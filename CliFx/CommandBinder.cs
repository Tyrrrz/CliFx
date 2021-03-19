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
            IMemberSchema memberSchema,
            string? rawValue,
            Type targetType)
        {
            // Custom converter
            if (memberSchema.ConverterType is not null)
            {
                var converter = (IBindingConverter) _typeActivator.CreateInstance(memberSchema.ConverterType);
                return converter.Convert(rawValue);
            }

            // Assignable from string (e.g. string itself, object, etc)
            if (targetType.IsAssignableFrom(typeof(string)))
            {
                return rawValue;
            }

            // Special case for bool
            if (targetType == typeof(bool))
            {
                return string.IsNullOrWhiteSpace(rawValue) || bool.Parse(rawValue);
            }

            // IConvertible primitives (int, double, char, etc)
            if (targetType.IsConvertible())
            {
                return Convert.ChangeType(rawValue, targetType, FormatProvider);
            }

            // Special case for DateTimeOffset
            if (targetType == typeof(DateTimeOffset))
            {
                return DateTimeOffset.Parse(rawValue, FormatProvider);
            }

            // Special case for TimeSpan
            if (targetType == typeof(TimeSpan))
            {
                return TimeSpan.Parse(rawValue, FormatProvider);
            }

            // Enum
            if (targetType.IsEnum)
            {
                // Null reference exception will be handled upstream
                return Enum.Parse(targetType, rawValue!, true);
            }

            // Nullable<T>
            var nullableUnderlyingType = targetType.TryGetNullableUnderlyingType();
            if (nullableUnderlyingType is not null)
            {
                return !string.IsNullOrWhiteSpace(rawValue)
                    ? ConvertSingle(memberSchema, rawValue, nullableUnderlyingType)
                    : null;
            }

            // String-constructible (FileInfo, etc)
            var stringConstructor = targetType.GetConstructor(new[] {typeof(string)});
            if (stringConstructor is not null)
            {
                return stringConstructor.Invoke(new object?[] {rawValue});
            }

            // String-parseable (with IFormatProvider)
            var parseMethodWithFormatProvider = targetType.TryGetStaticParseMethod(true);
            if (parseMethodWithFormatProvider is not null)
            {
                return parseMethodWithFormatProvider.Invoke(null, new object?[] {rawValue, FormatProvider});
            }

            // String-parseable (without IFormatProvider)
            var parseMethod = targetType.TryGetStaticParseMethod();
            if (parseMethod is not null)
            {
                return parseMethod.Invoke(null, new object?[] {rawValue});
            }

            throw CliFxException.InternalError(
                $"{memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} has an unsupported type."
            );
        }

        private object? ConvertMultiple(
            IMemberSchema memberSchema,
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

            throw CliFxException.InternalError(
                $"{memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} has an unsupported non-scalar type." +
                Environment.NewLine +
                "Non-scalar type must be assignable from an array or have a public constructor that accepts an array."
            );
        }

        private object? ConvertMember(
            IMemberSchema memberSchema,
            Type targetType,
            IReadOnlyList<string> rawValues)
        {
            try
            {
                // Non-scalar
                var enumerableUnderlyingType = targetType.TryGetEnumerableUnderlyingType();
                if (targetType != typeof(string) && enumerableUnderlyingType is not null)
                {
                    return ConvertMultiple(memberSchema, rawValues, targetType, enumerableUnderlyingType);
                }

                // Scalar
                if (rawValues.Count <= 1)
                {
                    return ConvertSingle(memberSchema, rawValues.SingleOrDefault(), targetType);
                }
            }
            catch (Exception ex) when (ex is not CliFxException)
            {
                throw CliFxException.UserError(
                    $"{memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} cannot be bound from provided value(s):" +
                    Environment.NewLine +
                    rawValues.Select(v => v.Quote()).JoinToString(", ") +
                    Environment.NewLine +
                    $"Error: {ex.Message}",
                    ex
                );
            }

            // Mismatch (scalar but too many values)
            throw CliFxException.UserError(
                $"{memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} expects a single value, but provided with multiple:" +
                Environment.NewLine +
                rawValues.Select(v => v.Quote()).JoinToString(", ")
            );
        }

        private void ValidateMember(
            IMemberSchema memberSchema,
            object? convertedValue)
        {
            var errors = new List<BindingValidationError>();

            foreach (var validatorType in memberSchema.ValidatorTypes)
            {
                var validator = (IBindingValidator) _typeActivator.CreateInstance(validatorType);
                var error = validator.Validate(convertedValue);

                if (error is not null)
                    errors.Add(error);
            }

            if (errors.Any())
            {
                throw CliFxException.UserError(
                    $"{memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} cannot be bound from provided value(s):" +
                    Environment.NewLine +
                    errors.Select(e => "- " + e.Message).JoinToString(Environment.NewLine)
                );
            }
        }

        private void BindMember(
            IMemberSchema memberSchema,
            ICommand commandInstance,
            IReadOnlyList<string> rawValues)
        {
            var targetType = memberSchema.Property.Type;

            var convertedValue = ConvertMember(memberSchema, targetType, rawValues);
            ValidateMember(memberSchema, convertedValue);

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
                if (parameterSchema.Property.IsScalar())
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
            {
                throw CliFxException.UserError(
                    "Unexpected parameters provided:" +
                    Environment.NewLine +
                    remainingParameterInputs
                        .Select(p => p.GetFormattedIdentifier())
                        .JoinToString(Environment.NewLine)
                );
            }

            if (remainingParameterSchemas.Any())
            {
                throw CliFxException.UserError(
                    "Missing values for one or more parameters:" +
                    Environment.NewLine +
                    remainingParameterSchemas
                        .Select(o => o.GetFormattedIdentifier())
                        .JoinToString(Environment.NewLine)
                );
            }
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
                    .Where(o => optionSchema.MatchesIdentifier(o.Identifier))
                    .ToArray();

                var environmentVariableInput = commandInput
                    .EnvironmentVariables
                    .FirstOrDefault(e => optionSchema.MatchesEnvironmentVariable(e.Name));

                var rawValues = 0 switch
                {
                    // Direct input
                    _ when optionInputs.Any() => optionInputs.SelectMany(o => o.Values).ToArray(),

                    // Environment variable input
                    _ when environmentVariableInput is not null => optionSchema.Property.IsScalar()
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
            {
                throw CliFxException.UserError(
                    "Unrecognized options provided:" +
                    Environment.NewLine +
                    remainingOptionInputs
                        .Select(o => o.GetFormattedIdentifier())
                        .JoinToString(Environment.NewLine)
                );
            }

            if (remainingRequiredOptionSchemas.Any())
            {
                throw CliFxException.UserError(
                    "Missing values for one or more required options:" +
                    Environment.NewLine +
                    remainingRequiredOptionSchemas
                        .Select(o => o.GetFormattedIdentifier())
                        .JoinToString(Environment.NewLine)
                );
            }
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