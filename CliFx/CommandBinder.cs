using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CliFx.Exceptions;
using CliFx.Extensibility;
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
            // Sequence input with a sequence converter
            if (schema.IsSequence && schema.SequenceConverter is not null)
            {
                return schema.SequenceConverter.ConvertMany(rawValues);
            }

            // Sequence input without a sequence converter
            if (schema.IsSequence)
            {
                throw CliFxException.InternalError(
                    $"""
                    {schema.GetKind()} {schema.GetFormattedIdentifier()} is a sequence property but has no collection converter.
                    To fix this, use the source generator or provide a custom {nameof(
                        ISequenceBindingConverter
                    )} via the Converter attribute property.
                    """
                );
            }

            // Scalar input
            if (rawValues.Count <= 1)
            {
                var rawValue = rawValues.SingleOrDefault();

                return schema.Converter is not null ? schema.Converter.Convert(rawValue) : rawValue;
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
        IReadOnlyList<CommandParameterSchema> parameterSchemas,
        ICommand commandInstance,
        bool reportUnrecognizedAndMissing = true
    )
    {
        var remainingParameterInputs = commandInput.Parameters.ToList();
        var remainingRequiredParameterSchemas = parameterSchemas.Where(p => p.IsRequired).ToList();

        var position = 0;
        foreach (var parameterSchema in parameterSchemas.OrderBy(p => p.Order))
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

        if (reportUnrecognizedAndMissing && remainingParameterInputs.Any())
        {
            throw CliFxException.UserError(
                $"""
                Unrecognized parameter(s):
                {remainingParameterInputs.Select(p => p.GetFormattedIdentifier()).JoinToString(" ")}
                """
            );
        }

        if (reportUnrecognizedAndMissing && remainingRequiredParameterSchemas.Any())
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

    private void BindParameters(
        CommandInput commandInput,
        CommandSchema commandSchema,
        ICommand commandInstance
    ) => BindParameters(commandInput, commandSchema.Parameters, commandInstance);

    private void BindOptions(
        CommandInput commandInput,
        IReadOnlyList<CommandOptionSchema> optionSchemas,
        ICommand commandInstance,
        bool reportUnrecognizedAndMissing = true
    )
    {
        var remainingOptionInputs = commandInput.Options.ToList();
        var remainingRequiredOptionSchemas = optionSchemas.Where(o => o.IsRequired).ToList();

        foreach (var optionSchema in optionSchemas)
        {
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

        if (reportUnrecognizedAndMissing && remainingOptionInputs.Any())
        {
            throw CliFxException.UserError(
                $"""
                Unrecognized option(s):
                {remainingOptionInputs.Select(o => o.GetFormattedIdentifier()).JoinToString(", ")}
                """
            );
        }

        if (reportUnrecognizedAndMissing && remainingRequiredOptionSchemas.Any())
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

    private void BindOptions(
        CommandInput commandInput,
        CommandSchema commandSchema,
        ICommand commandInstance
    ) => BindOptions(commandInput, commandSchema.Options, commandInstance);

    public void BindHelpAndVersionOptions(
        CommandInput commandInput,
        CommandSchema commandSchema,
        ICommand commandInstance
    )
    {
        var options = new List<CommandOptionSchema>(2);

        if (commandInstance is ICommandWithHelpOption)
        {
            var option = commandSchema.Options.FirstOrDefault(o =>
                string.Equals(
                    o.Property.Name,
                    nameof(ICommandWithHelpOption.IsHelpRequested),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (option is not null)
                options.Add(option);
        }

        if (commandInstance is ICommandWithVersionOption)
        {
            var option = commandSchema.Options.FirstOrDefault(o =>
                string.Equals(
                    o.Property.Name,
                    nameof(ICommandWithVersionOption.IsVersionRequested),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (option is not null)
                options.Add(option);
        }

        if (!options.Any())
            return;

        BindOptions(commandInput, options, commandInstance, false);
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
