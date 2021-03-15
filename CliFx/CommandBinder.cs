using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Extensibility;
using CliFx.Infrastructure;
using CliFx.Input;
using CliFx.Schema;
using CliFx.Utils.Extensions;

namespace CliFx
{
    internal class CommandBinder
    {
        private static readonly IFormatProvider FormatProvider = CultureInfo.InvariantCulture;

        private readonly ITypeActivator _typeActivator;

        public CommandBinder(ITypeActivator typeActivator)
        {
            _typeActivator = typeActivator;
        }

        private object? ConvertScalar(
            MemberSchema memberSchema,
            string? value,
            Type targetType)
        {
            var converter = memberSchema.ConverterType is not null
                ? (IArgumentConverter) _typeActivator.CreateInstance(memberSchema.ConverterType)
                : null;

            return 0 switch
            {
                // Custom converter
                _ when converter is not null => converter.Convert(value),

                // Assignable from string (string, object, etc)
                _ when targetType.IsAssignableFrom(typeof(string)) => value,

                // Boolean (special case)
                _ when targetType == typeof(bool) => string.IsNullOrWhiteSpace(value) || bool.Parse(value),

                // Primitive types
                _ when targetType.IsConvertible() => Convert.ChangeType(value, targetType, FormatProvider),
                _ when targetType == typeof(DateTimeOffset) => DateTimeOffset.Parse(value, FormatProvider),
                _ when targetType == typeof(TimeSpan) => TimeSpan.Parse(value, FormatProvider),

                // Enum types (allow null reference exceptions here, they will be caught)
                _ when targetType.IsEnum => Enum.Parse(targetType, value!, true),

                // Nullable value types
                _ when targetType.TryGetNullableUnderlyingType() is { } nullableUnderlyingType =>
                    !string.IsNullOrWhiteSpace(value)
                        ? ConvertScalar(memberSchema, value, nullableUnderlyingType)
                        : null,

                // String-constructible (FileInfo, DirectoryInfo, etc)
                _ when targetType.GetConstructor(new[] {typeof(string)}) is { } stringConstructor =>
                    stringConstructor.Invoke(new object?[] {value}),

                // String-parseable (Guid, IpAddress, etc)
                _ when targetType.TryGetStaticParseMethod(true) is { } parseMethod =>
                    parseMethod.Invoke(null, new object?[] {value, FormatProvider}),
                _ when targetType.TryGetStaticParseMethod() is { } parseMethod =>
                    parseMethod.Invoke(null, new object?[] {value}),

                // No conversion available
                _ => throw CliFxException.CannotConvertToType(memberSchema, value, targetType)
            };
        }

        private object? ConvertNonScalar(
            MemberSchema memberSchema,
            IReadOnlyList<string> values,
            Type targetEnumerableType,
            Type targetElementType)
        {
            var array = values
                .Select(v => ConvertScalar(memberSchema, v, targetElementType))
                .ToNonGenericArray(targetElementType);

            var arrayType = array.GetType();

            return 0 switch
            {
                // Assignable from an array (T[], IReadOnlyList<T>, etc)
                _ when targetEnumerableType.IsAssignableFrom(arrayType) => array,

                // Array-constructible (List<T>, HashSet<T>, etc)
                _ when targetEnumerableType.GetConstructor(new[] {arrayType}) is { } arrayConstructor =>
                    arrayConstructor.Invoke(new object?[] {array}),

                // No conversion available
                _ => throw CliFxException.CannotConvertNonScalar(memberSchema, values, targetEnumerableType)
            };
        }

        private void BindMember(
            MemberSchema memberSchema,
            ICommand commandInstance,
            IReadOnlyList<string> values)
        {
            if (memberSchema.Property is null)
                return;

            var targetType = memberSchema.Property.PropertyType;

            // Convert
            var convertedValue = 0 switch
            {
                // Non-scalar
                _ when
                    targetType != typeof(string) &&
                    targetType.TryGetEnumerableUnderlyingType() is { } enumerableUnderlyingType =>
                    ConvertNonScalar(memberSchema, values, targetType, enumerableUnderlyingType),

                // Scalar
                _ when values.Count <= 1 =>
                    ConvertScalar(memberSchema, values.SingleOrDefault(), targetType),

                // Mismatch (scalar but too many values)
                _ => throw CliFxException.CannotConvertMultipleValuesToNonScalar(memberSchema, values)
            };

            // Validate
            foreach (var validatorType in memberSchema.ValidatorTypes)
            {
                var validator = (IArgumentValidator) _typeActivator.CreateInstance(validatorType);
                validator.Validate(convertedValue);
            }

            memberSchema.Property.SetValue(commandInstance, convertedValue);
        }

        private void BindParameters(
            CommandSchema commandSchema,
            CommandInput commandInput,
            ICommand commandInstance)
        {
            // Keep track of remaining inputs to make sure that
            // all parameters have been bound.
            var remainingParameterInputs = commandInput.Parameters.ToList();

            // Scalar parameters
            var scalarParameters = commandSchema
                .Parameters
                .OrderBy(p => p.Order)
                .TakeWhile(p => p.IsScalar)
                .ToArray();

            for (var i = 0; i < scalarParameters.Length; i++)
            {
                var parameterSchema = scalarParameters[i];

                var scalarInput = i < commandInput.Parameters.Count
                    ? commandInput.Parameters[i]
                    : throw CliFxException.ParameterNotSet(parameterSchema);

                BindMember(parameterSchema, commandInstance, new[] {scalarInput.Value});
                remainingParameterInputs.Remove(scalarInput);
            }

            // Non-scalar parameter (only one is allowed)
            var nonScalarParameter = commandSchema
                .Parameters
                .OrderBy(p => p.Order)
                .FirstOrDefault(p => !p.IsScalar);

            if (nonScalarParameter is not null)
            {
                var nonScalarValues = commandInput.Parameters
                    .Skip(scalarParameters.Length)
                    .Select(p => p.Value)
                    .ToArray();

                // Parameters are required by default so a non-scalar parameter must
                // be bound to at least one value.
                if (!nonScalarValues.Any())
                    throw CliFxException.ParameterNotSet(nonScalarParameter);

                BindMember(nonScalarParameter, commandInstance, nonScalarValues);
                remainingParameterInputs.Clear();
            }

            // Ensure all inputs were bound
            if (remainingParameterInputs.Any())
                throw CliFxException.UnrecognizedParametersProvided(remainingParameterInputs);
        }

        private void BindOptions(
            CommandSchema commandSchema,
            CommandInput commandInput,
            ICommand commandInstance)
        {
            // Keep track of remaining inputs to make sure that
            // all options have been bound.
            var remainingOptionInputs = commandInput.Options.ToList();

            // Keep track of remaining unset required options
            var unsetRequiredOptionSchemas = commandSchema.Options.Where(o => o.IsRequired).ToList();

            // Environment variables
            foreach (var environmentVariableInput in commandInput.EnvironmentVariables)
            {
                var optionSchema = commandSchema
                    .Options
                    .FirstOrDefault(o => o.MatchesEnvironmentVariable(environmentVariableInput.Name));

                if (optionSchema is null)
                    continue;

                var values = optionSchema.IsScalar
                    ? new[] {environmentVariableInput.GetValue()}
                    : environmentVariableInput.GetValues();

                BindMember(optionSchema, commandInstance, values);
                unsetRequiredOptionSchemas.Remove(optionSchema);
            }

            // Direct input
            foreach (var optionSchema in commandSchema.Options)
            {
                var inputs = commandInput
                    .Options
                    .Where(i => optionSchema.MatchesNameOrShortName(i.Identifier))
                    .ToArray();

                // Skip if the inputs weren't provided for this option
                if (!inputs.Any())
                    continue;

                var inputValues = inputs.SelectMany(i => i.Values).ToArray();
                BindMember(optionSchema, commandInstance, inputValues);

                remainingOptionInputs.RemoveRange(inputs);

                // Required option implies that the value has to be set and also be non-empty
                if (inputValues.Any())
                    unsetRequiredOptionSchemas.Remove(optionSchema);
            }

            // Ensure all inputs were bound
            if (remainingOptionInputs.Any())
                throw CliFxException.UnrecognizedOptionsProvided(remainingOptionInputs);

            // Ensure all required options were set
            if (unsetRequiredOptionSchemas.Any())
                throw CliFxException.RequiredOptionsNotSet(unsetRequiredOptionSchemas);
        }

        public void Bind(CommandSchema commandSchema, CommandInput commandInput, ICommand commandInstance)
        {
            BindParameters(commandSchema, commandInput, commandInstance);
            BindOptions(commandSchema, commandInput, commandInstance);
        }
    }
}