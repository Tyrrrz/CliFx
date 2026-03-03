using System;
using System.Collections.Generic;
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
    private static object? ConvertSingle(CommandInputSchema schema, string? rawValue)
    {
        if (schema.Converter is null)
            throw CliFxException.InternalError(
                $"""
                {schema.GetKind()} {schema.GetFormattedIdentifier()} has an unsupported underlying property type.
                There is no known way to convert a string value into an instance of type `{schema.Property.Type.FullName}`.
                To fix this, either change the property to use a supported type or configure a custom converter.
                """
            );

        return schema.Converter.Convert(rawValue);
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses dynamic array creation. Use a custom IBindingConverter for AOT compatibility."
    )]
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode(
        "Uses dynamic array creation. Use a custom IBindingConverter for AOT compatibility."
    )]
    private object? ConvertMultiple(
        CommandInputSchema schema,
        IReadOnlyList<string> rawValues,
        Type targetEnumerableType,
        Type targetElementType
    )
    {
        var array = rawValues
            .Select(v => ConvertSingle(schema, v))
            .ToNonGenericArray(targetElementType);

        var arrayType = array.GetType();

        if (targetEnumerableType.IsAssignableFrom(arrayType))
        {
            return array;
        }

        var arrayConstructor = targetEnumerableType.GetConstructor([arrayType]);
        if (arrayConstructor is not null)
        {
            return arrayConstructor.Invoke([array]);
        }

        throw CliFxException.InternalError(
            $"""
            {schema.GetKind()} {schema.GetFormattedIdentifier()} has an unsupported underlying property type.
            There is no known way to convert an array of `{targetElementType.FullName}` into an instance of type `{targetEnumerableType.FullName}`.
            To fix this, change the property to use a type which can be assigned from an array or a type which has a constructor that accepts an array.
            """
        );
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses reflection to bind values. Use a custom IBindingConverter for AOT compatibility."
    )]
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode(
        "Uses dynamic array creation. Use a custom IBindingConverter for AOT compatibility."
    )]
    private object? ConvertMember(CommandInputSchema schema, IReadOnlyList<string> rawValues)
    {
        try
        {
            var propertyType = schema.Property.Type;

            // Non-scalar (sequence)
            if (schema.IsSequence)
            {
                var enumerableUnderlyingType = propertyType.TryGetEnumerableUnderlyingType();
                if (enumerableUnderlyingType is not null)
                {
                    return ConvertMultiple(
                        schema,
                        rawValues,
                        propertyType,
                        enumerableUnderlyingType
                    );
                }
            }

            // Scalar
            if (rawValues.Count <= 1)
            {
                return ConvertSingle(schema, rawValues.SingleOrDefault());
            }
        }
        catch (Exception ex) when (ex is not CliFxException)
        {
            var errorMessage = ex is TargetInvocationException invokeEx
                ? invokeEx.InnerException?.Message ?? invokeEx.Message
                : ex.Message;

            throw CliFxException.UserError(
                $"""
                {schema.GetKind()} {schema.GetFormattedIdentifier()} cannot be set from the provided argument(s):
                {rawValues.Select(v => '<' + v + '>').JoinToString(" ")}
                Error: {errorMessage}
                """,
                ex
            );
        }

        // Mismatch (scalar but too many values)
        throw CliFxException.UserError(
            $"""
            {schema.GetKind()} {schema.GetFormattedIdentifier()} expects a single argument, but provided with multiple:
            {rawValues.Select(v => '<' + v + '>').JoinToString(" ")}
            """
        );
    }

    private void ValidateMember(CommandInputSchema schema, object? convertedValue)
    {
        var errors = new List<BindingValidationError>();

        foreach (var validator in schema.Validators)
        {
            var error = validator.Validate(convertedValue);
            if (error is not null)
                errors.Add(error);
        }

        if (errors.Any())
        {
            throw CliFxException.UserError(
                $"""
                {schema.GetKind()} {schema.GetFormattedIdentifier()} has been provided with an invalid value.
                Error(s):
                {errors.Select(e => "- " + e.Message).JoinToString(Environment.NewLine)}
                """
            );
        }
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses reflection to convert values. Use a custom IBindingConverter for AOT compatibility."
    )]
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode(
        "Uses dynamic array creation. Use a custom IBindingConverter for AOT compatibility."
    )]
    private void BindMember(
        CommandInputSchema schema,
        ICommand commandInstance,
        IReadOnlyList<string> rawValues
    )
    {
        var convertedValue = ConvertMember(schema, rawValues);
        ValidateMember(schema, convertedValue);
        schema.Property.SetValue(commandInstance, convertedValue);
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses reflection to bind values. Use a custom IBindingConverter for AOT compatibility."
    )]
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode(
        "Uses dynamic array creation. Use a custom IBindingConverter for AOT compatibility."
    )]
    private void BindParameters(
        CommandInput commandInput,
        CommandSchema commandSchema,
        ICommand commandInstance
    )
    {
        var remainingParameterInputs = commandInput.Parameters.ToList();
        var remainingRequiredParameterSchemas = commandSchema
            .Parameters.Where(p => p.IsRequired)
            .ToList();

        var position = 0;

        foreach (var parameterSchema in commandSchema.Parameters.OrderBy(p => p.Order))
        {
            if (position >= commandInput.Parameters.Count)
                break;

            if (!parameterSchema.IsSequence)
            {
                var parameterInput = commandInput.Parameters[position];
                BindMember(parameterSchema, commandInstance, [parameterInput.Value]);

                position++;
                remainingParameterInputs.Remove(parameterInput);
            }
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

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses reflection to bind values. Use a custom IBindingConverter for AOT compatibility."
    )]
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode(
        "Uses dynamic array creation. Use a custom IBindingConverter for AOT compatibility."
    )]
    private void BindOptions(
        CommandInput commandInput,
        CommandSchema commandSchema,
        ICommand commandInstance
    )
    {
        var remainingOptionInputs = commandInput.Options.ToList();
        var remainingRequiredOptionSchemas = commandSchema
            .Options.Where(o => o.IsRequired)
            .ToList();

        foreach (var optionSchema in commandSchema.Options)
        {
            // Skip implicit options (no property binding)
            if (optionSchema.Property is NullPropertyBinding)
                continue;

            var optionInputs = commandInput
                .Options.Where(o => optionSchema.MatchesIdentifier(o.Identifier))
                .ToArray();

            var environmentVariableInput = commandInput.EnvironmentVariables.FirstOrDefault(e =>
                optionSchema.MatchesEnvironmentVariable(e.Name)
            );

            if (optionInputs.Any())
            {
                var rawValues = optionInputs.SelectMany(o => o.Values).ToArray();

                BindMember(optionSchema, commandInstance, rawValues);

                if (rawValues.Any())
                    remainingRequiredOptionSchemas.Remove(optionSchema);
            }
            else if (environmentVariableInput is not null)
            {
                var rawValues = !optionSchema.IsSequence
                    ? [environmentVariableInput.Value]
                    : environmentVariableInput.SplitValues();

                BindMember(optionSchema, commandInstance, rawValues);

                if (rawValues.Any())
                    remainingRequiredOptionSchemas.Remove(optionSchema);
            }
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

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses reflection to bind values. Use a custom IBindingConverter for AOT compatibility."
    )]
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode(
        "Uses dynamic array creation. Use a custom IBindingConverter for AOT compatibility."
    )]
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
