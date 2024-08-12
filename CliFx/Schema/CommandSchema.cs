using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Input;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

/// <summary>
/// Describes an individual command, along with its inputs.
/// </summary>
public class CommandSchema(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type,
    string? name,
    string? description,
    IReadOnlyList<CommandInputSchema> inputs
)
{
    /// <summary>
    /// Underlying CLR type of the command.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type Type { get; } = type;

    /// <summary>
    /// Command name.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Whether the command is the application's default command.
    /// </summary>
    public bool IsDefault { get; } = string.IsNullOrWhiteSpace(name);

    /// <summary>
    /// Command description.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Command inputs.
    /// </summary>
    public IReadOnlyList<CommandInputSchema> Inputs { get; } = inputs;

    /// <summary>
    /// Parameter inputs of the command.
    /// </summary>
    public IReadOnlyList<CommandParameterSchema> Parameters { get; } =
        inputs.OfType<CommandParameterSchema>().ToArray();

    /// <summary>
    /// Option inputs of the command.
    /// </summary>
    public IReadOnlyList<CommandOptionSchema> Options { get; } =
        inputs.OfType<CommandOptionSchema>().ToArray();

    internal bool MatchesName(string? name) =>
        !string.IsNullOrWhiteSpace(Name)
            ? string.Equals(name, Name, StringComparison.OrdinalIgnoreCase)
            : string.IsNullOrWhiteSpace(name);

    internal IReadOnlyDictionary<CommandInputSchema, object?> GetValues(ICommand instance)
    {
        var result = new Dictionary<CommandInputSchema, object?>();

        foreach (var parameterSchema in Parameters)
        {
            var value = parameterSchema.Property.GetValue(instance);
            result[parameterSchema] = value;
        }

        foreach (var optionSchema in Options)
        {
            var value = optionSchema.Property.GetValue(instance);
            result[optionSchema] = value;
        }

        return result;
    }

    private void ActivateParameters(ICommand instance, CommandInput input)
    {
        // Ensure there are no unexpected parameters and that all parameters are provided
        var remainingParameterInputs = input.Parameters.ToList();
        var remainingRequiredParameterSchemas = Parameters.Where(p => p.IsRequired).ToList();

        var position = 0;

        foreach (var parameterSchema in Parameters.OrderBy(p => p.Order))
        {
            // Break when there are no remaining inputs
            if (position >= input.Parameters.Count)
                break;

            // Non-sequence: take one input at the current position
            if (!parameterSchema.IsSequence)
            {
                var parameterInput = input.Parameters[position];
                parameterSchema.Activate(instance, [parameterInput.Value]);

                position++;
                remainingParameterInputs.Remove(parameterInput);
            }
            // Sequence: take all remaining inputs starting from the current position
            else
            {
                var parameterInputs = input.Parameters.Skip(position).ToArray();

                parameterSchema.Activate(instance, parameterInputs.Select(p => p.Value).ToArray());

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
                    .Select(p => p.FormattedIdentifier)
                    .JoinToString(" ")}
                """
            );
        }
    }

    private void ActivateOptions(ICommand instance, CommandInput input)
    {
        // Ensure there are no unrecognized options and that all required options are set
        var remainingOptionInputs = input.Options.ToList();
        var remainingRequiredOptionSchemas = Options.Where(o => o.IsRequired).ToList();

        foreach (var optionSchema in Options)
        {
            var optionInputs = input
                .Options.Where(o => optionSchema.MatchesIdentifier(o.Identifier))
                .ToArray();

            var environmentVariableInput = input.EnvironmentVariables.FirstOrDefault(e =>
                optionSchema.MatchesEnvironmentVariable(e.Name)
            );

            // Direct input
            if (optionInputs.Any())
            {
                var rawValues = optionInputs.SelectMany(o => o.Values).ToArray();

                optionSchema.Activate(instance, rawValues);

                // Required options need at least one value to be set
                if (rawValues.Any())
                    remainingRequiredOptionSchemas.Remove(optionSchema);
            }
            // Environment variable
            else if (environmentVariableInput is not null)
            {
                var rawValues = !optionSchema.IsSequence
                    ? [environmentVariableInput.Value]
                    : environmentVariableInput.SplitValues();

                optionSchema.Activate(instance, rawValues);

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
                {remainingOptionInputs.Select(o => o.FormattedIdentifier).JoinToString(", ")}
                """
            );
        }

        if (remainingRequiredOptionSchemas.Any())
        {
            throw CliFxException.UserError(
                $"""
                Missing required option(s):
                {remainingRequiredOptionSchemas
                    .Select(o => o.FormattedIdentifier)
                    .JoinToString(", ")}
                """
            );
        }
    }

    internal void Activate(ICommand instance, CommandInput input)
    {
        ActivateParameters(instance, input);
        ActivateOptions(instance, input);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => Name ?? "<default>";
}

/// <inheritdoc cref="CommandSchema" />
/// <remarks>
/// Generic version of the type is used to simplify initialization from source-generated code and
/// to enforce static references to all types used in the binding.
/// The non-generic version is used internally by the framework when operating in a dynamic context.
/// </remarks>
public class CommandSchema<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand
>(string? name, string? description, IReadOnlyList<CommandInputSchema> inputs)
    : CommandSchema(typeof(TCommand), name, description, inputs)
    where TCommand : ICommand;
