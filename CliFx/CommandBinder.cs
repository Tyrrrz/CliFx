using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CliFx.Exceptions;
using CliFx.Extensibility;
using CliFx.Infrastructure;
using CliFx.Input;
using CliFx.Schema;
using CliFx.Utils.Extensions;

namespace CliFx;

internal class CommandBinder
{
    private readonly ITypeActivator _typeActivator;
    private readonly IFormatProvider _formatProvider = CultureInfo.InvariantCulture;

    public CommandBinder(ITypeActivator typeActivator)
    {
        _typeActivator = typeActivator;
    }

    private object? ConvertSingle(IMemberSchema memberSchema, string? rawValue, Type targetType)
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
            return Convert.ChangeType(rawValue, targetType, _formatProvider);
        }

        // Special case for DateTimeOffset
        if (targetType == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(rawValue, _formatProvider);
        }

        // Special case for TimeSpan
        if (targetType == typeof(TimeSpan))
        {
            return TimeSpan.Parse(rawValue, _formatProvider);
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
            return parseMethodWithFormatProvider.Invoke(null, new object?[] {rawValue, _formatProvider});
        }

        // String-parseable (without IFormatProvider)
        var parseMethod = targetType.TryGetStaticParseMethod();
        if (parseMethod is not null)
        {
            return parseMethod.Invoke(null, new object?[] {rawValue});
        }

        throw CliFxException.InternalError(
            $"{memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} has an unsupported underlying property type." +
            Environment.NewLine +
            $"There is no known way to convert a string value into an instance of type `{targetType.FullName}`." +
            Environment.NewLine +
            "To fix this, either change the property to use a supported type or configure a custom converter."
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
            $"{memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} has an unsupported underlying property type." +
            Environment.NewLine +
            $"There is no known way to convert an array of `{targetElementType.FullName}` into an instance of type `{targetEnumerableType.FullName}`." +
            Environment.NewLine +
            "To fix this, change the property to use a type which can be assigned from an array or a type that has a constructor which accepts an array."
        );
    }

    private object? ConvertMember(IMemberSchema memberSchema, IReadOnlyList<string> rawValues)
    {
        var targetType = memberSchema.Property.Type;

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
        catch (Exception ex) when (ex is not CliFxException) // don't wrap CliFxException
        {
            // We use reflection-based invocation which can throw TargetInvocationException.
            // Unwrap these exceptions to provide a more user-friendly error message.
            var errorMessage = ex is TargetInvocationException invokeEx
                ? invokeEx.InnerException?.Message ?? invokeEx.Message
                : ex.Message;

            throw CliFxException.UserError(
                $"{memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} cannot be set from provided argument(s):" +
                Environment.NewLine +
                rawValues.Select(v => '<' + v + '>').JoinToString(" ") +
                Environment.NewLine +
                $"Error: {errorMessage}",
                ex
            );
        }

        // Mismatch (scalar but too many values)
        throw CliFxException.UserError(
            $"{memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} expects a single argument, but provided with multiple:" +
            Environment.NewLine +
            rawValues.Select(v => '<' + v + '>').JoinToString(" ")
        );
    }

    private void ValidateMember(IMemberSchema memberSchema, object? convertedValue)
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
                $"{memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} has been provided with an invalid value." +
                Environment.NewLine +
                "Error(s):" +
                Environment.NewLine +
                errors.Select(e => "- " + e.Message).JoinToString(Environment.NewLine)
            );
        }
    }

    private void BindMember(IMemberSchema memberSchema, ICommand commandInstance, IReadOnlyList<string> rawValues)
    {
        var convertedValue = ConvertMember(memberSchema, rawValues);
        ValidateMember(memberSchema, convertedValue);

        memberSchema.Property.SetValue(commandInstance, convertedValue);
    }

    private void BindParameters(CommandInput commandInput, CommandSchema commandSchema, ICommand commandInstance)
    {
        // Ensure there are no unexpected parameters and that all parameters are provided
        var remainingParameterInputs = commandInput.Parameters.ToList();
        var remainingRequiredParameterSchemas = commandSchema.Parameters.Where(q=> !q.IsOptional).ToList();

        var position = 0;

        foreach (var parameterSchema in commandSchema.Parameters.OrderBy(p => p.Order))
        {
            // Break when there are no remaining inputs
            if (position >= commandInput.Parameters.Count)
                break;

            // Scalar - take one input at the current position
            if (parameterSchema.Property.IsScalar())
            {
                var parameterInput = commandInput.Parameters[position];

                var rawValues = new[] {parameterInput.Value};
                BindMember(parameterSchema, commandInstance, rawValues);

                position++;

                remainingParameterInputs.Remove(parameterInput);
            }
            // Non-scalar - take all remaining inputs starting from the current position
            else
            {
                var parameterInputs = commandInput.Parameters.Skip(position).ToArray();

                var rawValues = parameterInputs.Select(p => p.Value).ToArray();
                BindMember(parameterSchema, commandInstance, rawValues);

                position += parameterInputs.Length;

                remainingParameterInputs.RemoveRange(parameterInputs);
            }

            remainingRequiredParameterSchemas.Remove(parameterSchema);
        }

        if (remainingParameterInputs.Any())
        {
            throw CliFxException.UserError(
                "Unexpected parameter(s):" +
                Environment.NewLine +
                remainingParameterInputs
                    .Select(p => p.GetFormattedIdentifier())
                    .JoinToString(" ")
            );
        }

        if (remainingRequiredParameterSchemas.Any())
        {
            throw CliFxException.UserError(
                "Missing parameter(s):" +
                Environment.NewLine +
                remainingRequiredParameterSchemas
                    .Select(o => o.GetFormattedIdentifier())
                    .JoinToString(" ")
            );
        }
    }

    private void BindOptions(CommandInput commandInput, CommandSchema commandSchema, ICommand commandInstance)
    {
        // Ensure there are no unrecognized options and that all required options are set
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

            // Direct input
            if (optionInputs.Any())
            {
                var rawValues = optionInputs.SelectMany(o => o.Values).ToArray();

                BindMember(optionSchema, commandInstance, rawValues);

                // Required options require at least one value to be set
                if (rawValues.Any())
                    remainingRequiredOptionSchemas.Remove(optionSchema);
            }
            // Environment variable
            else if (environmentVariableInput is not null)
            {
                var rawValues = optionSchema.Property.IsScalar()
                    ? new[] {environmentVariableInput.Value}
                    : environmentVariableInput.SplitValues();

                BindMember(optionSchema, commandInstance, rawValues);

                // Required options require at least one value to be set
                if (rawValues.Any())
                    remainingRequiredOptionSchemas.Remove(optionSchema);
            }
            // No input - skip
            else
            {
                continue;
            }

            remainingOptionInputs.RemoveRange(optionInputs);
        }

        if (remainingOptionInputs.Any())
        {
            throw CliFxException.UserError(
                "Unrecognized option(s):" +
                Environment.NewLine +
                remainingOptionInputs
                    .Select(o => o.GetFormattedIdentifier())
                    .JoinToString(", ")
            );
        }

        if (remainingRequiredOptionSchemas.Any())
        {
            throw CliFxException.UserError(
                "Missing required option(s):" +
                Environment.NewLine +
                remainingRequiredOptionSchemas
                    .Select(o => o.GetFormattedIdentifier())
                    .JoinToString(", ")
            );
        }
    }

    public void Bind(CommandInput commandInput, CommandSchema commandSchema, ICommand commandInstance)
    {
        BindParameters(commandInput, commandSchema, commandInstance);
        BindOptions(commandInput, commandSchema, commandInstance);
    }
}