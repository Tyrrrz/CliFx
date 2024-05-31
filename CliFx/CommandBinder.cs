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

internal class CommandBinder(ITypeActivator typeActivator)
{
    private readonly IFormatProvider _formatProvider = CultureInfo.InvariantCulture;

    private object? ConvertSingle(IInputSchema inputSchema, string? rawValue, Type targetType)
    {
        // Custom converter
        if (inputSchema.Converter is not null)
        {
            return inputSchema.Converter.Convert(rawValue);
        }

        // Assignable from a string (e.g. string itself, object, etc)
        if (targetType.IsAssignableFrom(typeof(string)))
        {
            return rawValue;
        }

        // Special case for bool
        if (targetType == typeof(bool))
        {
            return string.IsNullOrWhiteSpace(rawValue) || bool.Parse(rawValue);
        }

        // Special case for DateTimeOffset
        if (targetType == typeof(DateTimeOffset))
        {
            // Null reference exception will be handled upstream
            return DateTimeOffset.Parse(rawValue!, _formatProvider);
        }

        // Special case for TimeSpan
        if (targetType == typeof(TimeSpan))
        {
            // Null reference exception will be handled upstream
            return TimeSpan.Parse(rawValue!, _formatProvider);
        }

        // Enum
        if (targetType.IsEnum)
        {
            // Null reference exception will be handled upstream
            return Enum.Parse(targetType, rawValue!, true);
        }

        // Convertible primitives (int, double, char, etc)
        if (targetType.Implements(typeof(IConvertible)))
        {
            return Convert.ChangeType(rawValue, targetType, _formatProvider);
        }

        // Nullable<T>
        var nullableUnderlyingType = targetType.TryGetNullableUnderlyingType();
        if (nullableUnderlyingType is not null)
        {
            return !string.IsNullOrWhiteSpace(rawValue)
                ? ConvertSingle(inputSchema, rawValue, nullableUnderlyingType)
                : null;
        }

        // String-constructable (FileInfo, etc)
        var stringConstructor = targetType.GetConstructor([typeof(string)]);
        if (stringConstructor is not null)
        {
            return stringConstructor.Invoke([rawValue]);
        }

        // String-parseable (with IFormatProvider)
        var parseMethodWithFormatProvider = targetType.TryGetStaticParseMethod(true);
        if (parseMethodWithFormatProvider is not null)
        {
            return parseMethodWithFormatProvider.Invoke(null, [rawValue, _formatProvider]);
        }

        // String-parseable (without IFormatProvider)
        var parseMethod = targetType.TryGetStaticParseMethod();
        if (parseMethod is not null)
        {
            return parseMethod.Invoke(null, [rawValue]);
        }

        throw CliFxException.InternalError(
            $"""
            {inputSchema.GetKind()} {inputSchema.GetFormattedIdentifier()} has an unsupported underlying property type.
            There is no known way to convert a string value into an instance of type `{targetType.FullName}`.
            To fix this, either change the property to use a supported type or configure a custom converter.
            """
        );
    }

    private object? ConvertMultiple(
        IInputSchema inputSchema,
        IReadOnlyList<string> rawValues,
        Type targetEnumerableType,
        Type targetElementType
    )
    {
        var array = rawValues
            .Select(v => ConvertSingle(inputSchema, v, targetElementType))
            .ToNonGenericArray(targetElementType);

        var arrayType = array.GetType();

        // Assignable from an array (T[], IReadOnlyList<T>, etc)
        if (targetEnumerableType.IsAssignableFrom(arrayType))
        {
            return array;
        }

        // Array-constructable (List<T>, HashSet<T>, etc)
        var arrayConstructor = targetEnumerableType.GetConstructor([arrayType]);
        if (arrayConstructor is not null)
        {
            return arrayConstructor.Invoke([array]);
        }

        throw CliFxException.InternalError(
            $"""
            {inputSchema.GetKind()} {inputSchema.GetFormattedIdentifier()} has an unsupported underlying property type.
            There is no known way to convert an array of `{targetElementType.FullName}` into an instance of type `{targetEnumerableType.FullName}`.
            To fix this, change the property to use a type which can be assigned from an array or a type which has a constructor that accepts an array.
            """
        );
    }

    private object? ConvertMember(IInputSchema inputSchema, IReadOnlyList<string> rawValues)
    {
        try
        {
            // Non-scalar
            var enumerableUnderlyingType =
                inputSchema.Property.Type.TryGetEnumerableUnderlyingType();

            if (enumerableUnderlyingType is not null && inputSchema.Property.Type != typeof(string))
            {
                return ConvertMultiple(
                    inputSchema,
                    rawValues,
                    inputSchema.Property.Type,
                    enumerableUnderlyingType
                );
            }

            // Scalar
            if (rawValues.Count <= 1)
            {
                return ConvertSingle(
                    inputSchema,
                    rawValues.SingleOrDefault(),
                    inputSchema.Property.Type
                );
            }
        }
        catch (Exception ex) when (ex is not CliFxException) // don't wrap CliFxException
        {
            // We use reflection-based invocation which can throw TargetInvocationException.
            // Unwrap those exceptions to provide a more user-friendly error message.
            var errorMessage = ex is TargetInvocationException invokeEx
                ? invokeEx.InnerException?.Message ?? invokeEx.Message
                : ex.Message;

            throw CliFxException.UserError(
                $"""
                {inputSchema.GetKind()} {inputSchema.GetFormattedIdentifier()} cannot be set from the provided argument(s):
                {rawValues.Select(v => '<' + v + '>').JoinToString(" ")}
                Error: {errorMessage}
                """,
                ex
            );
        }

        // Mismatch (scalar but too many values)
        throw CliFxException.UserError(
            $"""
            {inputSchema.GetKind()} {inputSchema.GetFormattedIdentifier()} expects a single argument, but provided with multiple:
            {rawValues.Select(v => '<' + v + '>').JoinToString(" ")}
            """
        );
    }

    private void ValidateMember(IInputSchema inputSchema, object? convertedValue)
    {
        var errors = new List<BindingValidationError>();

        foreach (var validatorType in inputSchema.Validators)
        {
            var validator = typeActivator.CreateInstance<IBindingValidator>(validatorType);
            var error = validator.Validate(convertedValue);

            if (error is not null)
                errors.Add(error);
        }

        if (errors.Any())
        {
            throw CliFxException.UserError(
                $"""
                {inputSchema.GetKind()} {inputSchema.GetFormattedIdentifier()} has been provided with an invalid value.
                Error(s):
                {errors.Select(e => "- " + e.Message).JoinToString(Environment.NewLine)}
                """
            );
        }
    }

    private void BindMember(
        IInputSchema inputSchema,
        ICommand commandInstance,
        IReadOnlyList<string> rawValues
    )
    {
        var convertedValue = ConvertMember(inputSchema, rawValues);
        ValidateMember(inputSchema, convertedValue);

        inputSchema.Property.SetValue(commandInstance, convertedValue);
    }

    private void BindParameters(
        CommandInput commandInput,
        CommandSchema commandSchema,
        ICommand commandInstance
    )
    {
        // Ensure there are no unexpected parameters and that all parameters are provided
        var remainingParameterInputs = commandInput.Parameters.ToList();
        var remainingRequiredParameterSchemas = commandSchema
            .Parameters.Where(p => p.IsRequired)
            .ToList();

        var position = 0;

        foreach (var parameterSchema in commandSchema.Parameters.OrderBy(p => p.Order))
        {
            // Break when there are no remaining inputs
            if (position >= commandInput.Parameters.Count)
                break;

            // Scalar: take one input at the current position
            if (parameterSchema.Property.IsScalar())
            {
                var parameterInput = commandInput.Parameters[position];
                BindMember(parameterSchema, commandInstance, [parameterInput.Value]);

                position++;
                remainingParameterInputs.Remove(parameterInput);
            }
            // Non-scalar: take all remaining inputs starting from the current position
            else
            {
                var parameterInputs = commandInput.Parameters.Skip(position).ToArray();

                BindMember(
                    parameterSchema,
                    commandInstance,
                    parameterInputs.Select(p => p.Value).ToArray()
                );

                position += parameterInputs.Length;
                remainingParameterInputs.RemoveRange(parameterInputs);
            }

            remainingRequiredParameterSchemas.Remove(parameterSchema);
        }

        if (remainingParameterInputs.Any())
        {
            throw CliFxException.UserError(
                $"""
                Unexpected parameter(s):
                {remainingParameterInputs.Select(p => p.GetFormattedIdentifier()).JoinToString(" ")}
                """
            );
        }

        if (remainingRequiredParameterSchemas.Any())
        {
            throw CliFxException.UserError(
                $"""
                Missing required parameter(s):
                {remainingRequiredParameterSchemas
                    .Select(p => p.GetFormattedIdentifier())
                    .JoinToString(" ")}
                """
            );
        }
    }

    private void BindOptions(
        CommandInput commandInput,
        CommandSchema commandSchema,
        ICommand commandInstance
    )
    {
        // Ensure there are no unrecognized options and that all required options are set
        var remainingOptionInputs = commandInput.Options.ToList();
        var remainingRequiredOptionSchemas = commandSchema
            .Options.Where(o => o.IsRequired)
            .ToList();

        foreach (var optionSchema in commandSchema.Options)
        {
            var optionInputs = commandInput
                .Options.Where(o => optionSchema.MatchesIdentifier(o.Identifier))
                .ToArray();

            var environmentVariableInput = commandInput.EnvironmentVariables.FirstOrDefault(e =>
                optionSchema.MatchesEnvironmentVariable(e.Name)
            );

            // Direct input
            if (optionInputs.Any())
            {
                var rawValues = optionInputs.SelectMany(o => o.Values).ToArray();

                BindMember(optionSchema, commandInstance, rawValues);

                // Required options need at least one value to be set
                if (rawValues.Any())
                    remainingRequiredOptionSchemas.Remove(optionSchema);
            }
            // Environment variable
            else if (environmentVariableInput is not null)
            {
                var rawValues = optionSchema.IsScalar
                    ? [environmentVariableInput.Value]
                    : environmentVariableInput.SplitValues();

                BindMember(optionSchema, commandInstance, rawValues);

                // Required options need at least one value to be set
                if (rawValues.Any())
                    remainingRequiredOptionSchemas.Remove(optionSchema);
            }
            // No input, skip
            else
            {
                continue;
            }

            remainingOptionInputs.RemoveRange(optionInputs);
        }

        if (remainingOptionInputs.Any())
        {
            throw CliFxException.UserError(
                $"""
                Unrecognized option(s):
                {remainingOptionInputs.Select(o => o.GetFormattedIdentifier()).JoinToString(", ")}
                """
            );
        }

        if (remainingRequiredOptionSchemas.Any())
        {
            throw CliFxException.UserError(
                $"""
                Missing required option(s):
                {remainingRequiredOptionSchemas
                    .Select(o => o.GetFormattedIdentifier())
                    .JoinToString(", ")}
                """
            );
        }
    }

    public void Bind(
        CommandInput commandInput,
        CommandSchema commandSchema,
        ICommand commandInstance
    )
    {
        BindParameters(commandInput, commandSchema, commandInstance);
        BindOptions(commandInput, commandSchema, commandInstance);
    }
}
