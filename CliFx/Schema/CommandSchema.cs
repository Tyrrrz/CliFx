using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Parsing;
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

        foreach (var parameter in Parameters)
        {
            var value = parameter.Property.Get(instance);
            result[parameter] = value;
        }

        foreach (var option in Options)
        {
            var value = option.Property.Get(instance);
            result[option] = value;
        }

        return result;
    }

    private void ActivateParameters(ICommand instance, CommandArguments arguments)
    {
        // Ensure there are no unexpected parameters and that all parameters are provided
        var remainingParameterTokens = arguments.Parameters.ToList();
        var remainingRequiredParameters = Parameters.Where(p => p.IsRequired).ToList();

        var position = 0;

        foreach (var parameter in Parameters.OrderBy(p => p.Order))
        {
            // Break when there are no remaining inputs
            if (position >= arguments.Parameters.Count)
                break;

            // Sequence: take all remaining inputs starting from the current position
            if (parameter.IsSequence)
            {
                var parameterTokens = arguments.Parameters.Skip(position).ToArray();

                parameter.Activate(instance, parameterTokens.Select(p => p.Value).ToArray());

                position += parameterTokens.Length;
                remainingParameterTokens.RemoveRange(parameterTokens);
            }
            // Non-sequence: take one input at the current position
            else
            {
                var parameterToken = arguments.Parameters[position];
                parameter.Activate(instance, [parameterToken.Value]);

                position++;
                remainingParameterTokens.Remove(parameterToken);
            }

            remainingRequiredParameters.Remove(parameter);
        }

        if (remainingParameterTokens.Any())
        {
            throw CliFxException.UserError(
                $"""
                Unexpected parameter(s):
                {remainingParameterTokens.Select(p => p.FormattedIdentifier).JoinToString(" ")}
                """
            );
        }

        if (remainingRequiredParameters.Any())
        {
            throw CliFxException.UserError(
                $"""
                Missing equired parameter(s):
                {remainingRequiredParameters
                    .Select(p => p.FormattedIdentifier)
                    .JoinToString(" ")}
                """
            );
        }
    }

    private void ActivateOptions(
        ICommand instance,
        CommandArguments arguments,
        IReadOnlyDictionary<string, string?> environmentVariables
    )
    {
        // Ensure there are no unrecognized options and that all required options are set
        var remainingOptionTokens = arguments.Options.ToList();
        var remainingRequiredOptions = Options.Where(o => o.IsRequired).ToList();

        foreach (var option in Options)
        {
            var optionToken = arguments
                .Options.Where(o => option.MatchesIdentifier(o.Identifier))
                .ToArray();

            var environmentVariable = environmentVariables.FirstOrDefault(v =>
                option.MatchesEnvironmentVariable(v.Key)
            );

            // Direct input
            if (optionToken.Any())
            {
                var rawValues = optionToken.SelectMany(o => o.Values).ToArray();

                option.Activate(instance, rawValues);

                // Required options need at least one value to be set
                if (rawValues.Any())
                    remainingRequiredOptions.Remove(option);
            }
            // Environment variable
            else if (!string.IsNullOrEmpty(environmentVariable.Value))
            {
                var rawValues = !option.IsSequence
                    ? [environmentVariable.Value]
                    : environmentVariable.Value.Split(
                        Path.PathSeparator,
                        StringSplitOptions.RemoveEmptyEntries
                    );

                option.Activate(instance, rawValues);

                // Required options need at least one value to be set
                if (rawValues.Any())
                    remainingRequiredOptions.Remove(option);
            }
            // No input, skip
            else
            {
                continue;
            }

            remainingOptionTokens.RemoveRange(optionToken);
        }

        if (remainingOptionTokens.Any())
        {
            throw CliFxException.UserError(
                $"""
                Unrecognized option(s):
                {remainingOptionTokens.Select(o => o.FormattedIdentifier).JoinToString(", ")}
                """
            );
        }

        if (remainingRequiredOptions.Any())
        {
            throw CliFxException.UserError(
                $"""
                Missing required option(s):
                {remainingRequiredOptions
                    .Select(o => o.FormattedIdentifier)
                    .JoinToString(", ")}
                """
            );
        }
    }

    internal void Activate(
        ICommand instance,
        CommandArguments arguments,
        IReadOnlyDictionary<string, string?> environmentVariables
    )
    {
        ActivateParameters(instance, arguments);
        ActivateOptions(instance, arguments, environmentVariables);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => Name ?? "{default}";
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
