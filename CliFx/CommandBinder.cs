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
    private object? Convert(CommandInputSchema schema, IReadOnlyList<string> rawValues)
    {
        try
        {
            // Non-scalar (sequence) with a collection converter (AOT-compatible path)
            if (schema.IsSequence && schema.CollectionConverter is not null)
            {
                return schema.CollectionConverter.ConvertMany(rawValues);
            }

            // Non-scalar (sequence) without a collection converter
            if (schema.IsSequence)
            {
                throw CliFxException.InternalError(
                    $"""
                    {schema.GetKind()} {schema.GetFormattedIdentifier()} is a sequence property but has no collection converter.
                    To fix this, use the source generator or provide a custom {nameof(
                        ICollectionBindingConverter
                    )} via the Converter attribute property.
                    """
                );
            }

            // Scalar
            if (rawValues.Count <= 1)
            {
                var rawValue = rawValues.SingleOrDefault();
                // Null converter means pass-through (used for string-typed properties)
                if (schema.Converter is null)
                    return rawValue;

                return schema.Converter.Convert(rawValue);
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

    private void BindMember(
        CommandInputSchema schema,
        ICommand commandInstance,
        IReadOnlyList<string> rawValues
    )
    {
        var convertedValue = Convert(schema, rawValues);
        ValidateMember(schema, convertedValue);
        try
        {
            schema.Property.SetValue(commandInstance, convertedValue);
        }
        catch (Exception ex) when (ex is not CliFxException)
        {
            var errorMessage = ex is TargetInvocationException invokeEx
                ? invokeEx.InnerException?.Message ?? ex.Message
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
    }

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

    public void Bind(
        CommandInput commandInput,
        CommandSchema commandSchema,
        ICommand commandInstance
    )
    {
        // Bind options first so that IsHelpRequested / IsVersionRequested are set
        // before parameter binding, enabling the caller to check them after Bind() returns.
        BindOptions(commandInput, commandSchema, commandInstance);
        BindParameters(commandInput, commandSchema, commandInstance);
    }
}
