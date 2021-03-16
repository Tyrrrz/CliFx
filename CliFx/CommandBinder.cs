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
    // TODO: cleanup
    internal class CommandBinder
    {
        private static readonly IFormatProvider FormatProvider = CultureInfo.InvariantCulture;

        private readonly ITypeActivator _typeActivator;

        public CommandBinder(ITypeActivator typeActivator)
        {
            _typeActivator = typeActivator;
        }

        private object? ConvertSingle(
            MemberSchema memberSchema,
            string? rawValue,
            Type targetType)
        {
            var converter = memberSchema.ConverterType is not null
                ? (IArgumentConverter) _typeActivator.CreateInstance(memberSchema.ConverterType)
                : null;

            return 0 switch
            {
                // Custom converter
                _ when converter is not null => converter.Convert(rawValue),

                // Assignable from string (string, object, etc)
                _ when targetType.IsAssignableFrom(typeof(string)) => rawValue,

                // Boolean (special case)
                _ when targetType == typeof(bool) => string.IsNullOrWhiteSpace(rawValue) || bool.Parse(rawValue),

                // Primitive types
                _ when targetType.IsConvertible() => Convert.ChangeType(rawValue, targetType, FormatProvider),
                _ when targetType == typeof(DateTimeOffset) => DateTimeOffset.Parse(rawValue, FormatProvider),
                _ when targetType == typeof(TimeSpan) => TimeSpan.Parse(rawValue, FormatProvider),

                // Enum types (allow null reference exceptions here, they will be caught)
                _ when targetType.IsEnum => Enum.Parse(targetType, rawValue!, true),

                // Nullable value types
                _ when targetType.TryGetNullableUnderlyingType() is { } nullableUnderlyingType =>
                    !string.IsNullOrWhiteSpace(rawValue)
                        ? ConvertSingle(memberSchema, rawValue, nullableUnderlyingType)
                        : null,

                // String-constructible (FileInfo, DirectoryInfo, etc)
                _ when targetType.GetConstructor(new[] {typeof(string)}) is { } stringConstructor =>
                    stringConstructor.Invoke(new object?[] {rawValue}),

                // String-parseable (Guid, IpAddress, etc)
                _ when targetType.TryGetStaticParseMethod(true) is { } parseMethod =>
                    parseMethod.Invoke(null, new object?[] {rawValue, FormatProvider}),
                _ when targetType.TryGetStaticParseMethod() is { } parseMethod =>
                    parseMethod.Invoke(null, new object?[] {rawValue}),

                // No conversion available
                _ => throw CliFxException.CannotConvertToType(memberSchema, rawValue, targetType)
            };
        }

        private object? ConvertMultiple(
            MemberSchema memberSchema,
            IReadOnlyList<string> rawValues,
            Type targetEnumerableType,
            Type targetElementType)
        {
            var array = rawValues
                .Select(v => ConvertSingle(memberSchema, v, targetElementType))
                .ToNonGenericArray(targetElementType);

            var arrayType = array.GetType();

            // Assignable from an array (T[], IReadOnlyList<T>, etc)
            if (targetEnumerableType.IsAssignableFrom(arrayType))
            {
                return array;
            }

            // Array-constructible (List<T>, HashSet<T>, etc)
            var arrayConstructor = targetEnumerableType.GetConstructor(new[] {arrayType});
            if (arrayConstructor is not null)
            {
                return arrayConstructor.Invoke(new object?[] {array});
            }

            throw CliFxException.CannotConvertNonScalar(memberSchema, rawValues, targetEnumerableType);
        }

        private void BindMember(
            MemberSchema memberSchema,
            ICommand commandInstance,
            IReadOnlyList<string> rawValues)
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
                    ConvertMultiple(memberSchema, rawValues, targetType, enumerableUnderlyingType),

                // Scalar
                _ when rawValues.Count <= 1 =>
                    ConvertSingle(memberSchema, rawValues.SingleOrDefault(), targetType),

                // Mismatch (scalar but too many values)
                _ => throw CliFxException.CannotConvertMultipleValuesToNonScalar(memberSchema, rawValues)
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
            CommandInput commandInput,
            CommandSchema commandSchema,
            ICommand commandInstance)
        {
            // Ensure there are no unexpected parameters or missing parameters
            var remainingParameterInputs = commandInput.Parameters.ToList();
            var remainingParameterSchemas = commandSchema.Parameters.ToList();

            var position = 0;

            foreach (var parameterSchema in commandSchema.Parameters.OrderBy(p => p.Order))
            {
                // Check bounds
                if (position >= commandInput.Parameters.Count)
                    break;

                // Scalar - take one input at current position
                if (parameterSchema.IsScalar)
                {
                    var parameterInput = commandInput.Parameters[position];

                    var rawValues = new[] {parameterInput.Value}; // TODO: possible to avoid creating array?
                    BindMember(parameterSchema, commandInstance, rawValues);

                    position++;

                    remainingParameterInputs.Remove(parameterInput);
                }
                // Non-scalar - take all remaining inputs starting at current position
                else
                {
                    var parameterInputs = commandInput.Parameters.Skip(position).ToArray();

                    var rawValues = parameterInputs.Select(p => p.Value).ToArray();
                    BindMember(parameterSchema, commandInstance, rawValues);

                    position += parameterInputs.Length;

                    remainingParameterInputs.RemoveRange(parameterInputs);
                }

                remainingParameterSchemas.Remove(parameterSchema);
            }

            if (remainingParameterInputs.Any())
                throw CliFxException.UnrecognizedParametersProvided(remainingParameterInputs);

            // TODO: fix exception
            if (remainingParameterSchemas.Any())
                throw CliFxException.ParameterNotSet(remainingParameterSchemas.First());
        }

        private void BindOptions(
            CommandInput commandInput,
            CommandSchema commandSchema,
            ICommand commandInstance)
        {
            // Ensure there are no unrecognized options or missing required options
            var remainingOptionInputs = commandInput.Options.ToList();
            var remainingRequiredOptionSchemas = commandSchema.Options.Where(o => o.IsRequired).ToList();

            foreach (var optionSchema in commandSchema.Options)
            {
                var optionInputs = commandInput
                    .Options
                    .Where(o => optionSchema.MatchesNameOrShortName(o.Identifier))
                    .ToArray();

                var environmentVariableInput = commandInput
                    .EnvironmentVariables
                    .FirstOrDefault(e => optionSchema.MatchesEnvironmentVariable(e.Name));

                var rawValues = 0 switch
                {
                    // Direct input
                    _ when optionInputs.Any() => optionInputs.SelectMany(o => o.Values).ToArray(),

                    // Environment variable input
                    _ when environmentVariableInput is not null => optionSchema.IsScalar
                        ? new[] {environmentVariableInput.Value}
                        : environmentVariableInput.SplitValues(),

                    // No input
                    _ => Array.Empty<string>()
                };

                BindMember(optionSchema, commandInstance, rawValues);

                remainingOptionInputs.RemoveRange(optionInputs);

                // Required options require at least one value to be set
                if (rawValues.Any())
                    remainingRequiredOptionSchemas.Remove(optionSchema);
            }

            if (remainingOptionInputs.Any())
                throw CliFxException.UnrecognizedOptionsProvided(remainingOptionInputs);

            if (remainingRequiredOptionSchemas.Any())
                throw CliFxException.RequiredOptionsNotSet(remainingRequiredOptionSchemas);
        }

        public void Bind(
            CommandInput commandInput,
            CommandSchema commandSchema,
            ICommand commandInstance)
        {
            BindParameters(commandInput, commandSchema, commandInstance);
            BindOptions(commandInput, commandSchema, commandInstance);
        }
    }
}