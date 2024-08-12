using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Utils.Extensions;

namespace CliFx.Input;

/// <summary>
/// Input provided by the user for a command.
/// </summary>
public partial class CommandInput(
    string? commandName,
    IReadOnlyList<CommandDirectiveInput> directives,
    IReadOnlyList<CommandParameterInput> parameters,
    IReadOnlyList<CommandOptionInput> options,
    IReadOnlyList<EnvironmentVariableInput> environmentVariables
)
{
    /// <summary>
    /// Name of the requested command.
    /// </summary>
    public string? CommandName { get; } = commandName;

    /// <summary>
    /// Provided directives.
    /// </summary>
    public IReadOnlyList<CommandDirectiveInput> Directives { get; } = directives;

    /// <summary>
    /// Provided parameters.
    /// </summary>
    public IReadOnlyList<CommandParameterInput> Parameters { get; } = parameters;

    /// <summary>
    /// Provided options.
    /// </summary>
    public IReadOnlyList<CommandOptionInput> Options { get; } = options;

    /// <summary>
    /// Provided environment variables.
    /// </summary>
    public IReadOnlyList<EnvironmentVariableInput> EnvironmentVariables { get; } =
        environmentVariables;

    internal bool IsDebugDirectiveSpecified => Directives.Any(d => d.IsDebugDirective);

    internal bool IsPreviewDirectiveSpecified => Directives.Any(d => d.IsPreviewDirective);
}

public partial class CommandInput
{
    private static IReadOnlyList<CommandDirectiveInput> ParseDirectives(
        IReadOnlyList<string> commandLineArguments,
        ref int index
    )
    {
        var result = new List<CommandDirectiveInput>();

        // Consume all consecutive directive arguments
        for (; index < commandLineArguments.Count; index++)
        {
            var argument = commandLineArguments[index];

            // Break on the first non-directive argument
            if (!argument.StartsWith('[') || !argument.EndsWith(']'))
                break;

            var directiveName = argument.Substring(1, argument.Length - 2);
            result.Add(new CommandDirectiveInput(directiveName));
        }

        return result;
    }

    private static string? ParseCommandName(
        IReadOnlyList<string> commandLineArguments,
        ISet<string> commandNames,
        ref int index
    )
    {
        var potentialCommandNameComponents = new List<string>();
        var commandName = default(string?);

        var lastIndex = index;

        // Append arguments to a buffer until we find the longest sequence that represents
        // a valid command name.
        for (var i = index; i < commandLineArguments.Count; i++)
        {
            var argument = commandLineArguments[i];

            potentialCommandNameComponents.Add(argument);

            var potentialCommandName = potentialCommandNameComponents.JoinToString(" ");
            if (commandNames.Contains(potentialCommandName))
            {
                // Record the position but continue the loop in case we find
                // a longer (more specific) match.
                commandName = potentialCommandName;
                lastIndex = i;
            }
        }

        // Move the index to the position where the command name ended
        if (!string.IsNullOrWhiteSpace(commandName))
            index = lastIndex + 1;

        return commandName;
    }

    private static IReadOnlyList<CommandParameterInput> ParseParameters(
        IReadOnlyList<string> commandLineArguments,
        ref int index
    )
    {
        var result = new List<CommandParameterInput>();

        // Consume all arguments until the first option identifier
        for (; index < commandLineArguments.Count; index++)
        {
            var argument = commandLineArguments[index];

            var isOptionIdentifier =
                // Name
                argument.StartsWith("--", StringComparison.Ordinal)
                    && argument.Length > 2
                    && char.IsLetter(argument[2])
                ||
                // Short name
                argument.StartsWith('-')
                    && argument.Length > 1
                    && char.IsLetter(argument[1]);

            // Break on the first option identifier
            if (isOptionIdentifier)
                break;

            result.Add(new CommandParameterInput(index, argument));
        }

        return result;
    }

    private static IReadOnlyList<CommandOptionInput> ParseOptions(
        IReadOnlyList<string> commandLineArguments,
        ref int index
    )
    {
        var result = new List<CommandOptionInput>();

        var lastOptionIdentifier = default(string?);
        var lastOptionValues = new List<string>();

        // Consume and group all remaining arguments into options
        for (; index < commandLineArguments.Count; index++)
        {
            var argument = commandLineArguments[index];

            // Name
            if (
                argument.StartsWith("--", StringComparison.Ordinal)
                && argument.Length > 2
                && char.IsLetter(argument[2])
            )
            {
                // Flush previous
                if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
                    result.Add(new CommandOptionInput(lastOptionIdentifier, lastOptionValues));

                lastOptionIdentifier = argument[2..];
                lastOptionValues = [];
            }
            // Short name
            else if (argument.StartsWith('-') && argument.Length > 1 && char.IsLetter(argument[1]))
            {
                foreach (var identifier in argument[1..])
                {
                    // Flush previous
                    if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
                        result.Add(new CommandOptionInput(lastOptionIdentifier, lastOptionValues));

                    lastOptionIdentifier = identifier.AsString();
                    lastOptionValues = [];
                }
            }
            // Value
            else if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
            {
                lastOptionValues.Add(argument);
            }
        }

        // Flush the last option
        if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
            result.Add(new CommandOptionInput(lastOptionIdentifier, lastOptionValues));

        return result;
    }

    internal static CommandInput Parse(
        IReadOnlyList<string> commandLineArguments,
        IReadOnlyDictionary<string, string> environmentVariables,
        IReadOnlyList<string> availableCommandNames
    )
    {
        var index = 0;

        var parsedDirectives = ParseDirectives(commandLineArguments, ref index);

        var parsedCommandName = ParseCommandName(
            commandLineArguments,
            availableCommandNames.ToHashSet(StringComparer.OrdinalIgnoreCase),
            ref index
        );

        var parsedParameters = ParseParameters(commandLineArguments, ref index);

        var parsedOptions = ParseOptions(commandLineArguments, ref index);

        var parsedEnvironmentVariables = environmentVariables
            .Select(kvp => new EnvironmentVariableInput(kvp.Key, kvp.Value))
            .ToArray();

        return new CommandInput(
            parsedCommandName,
            parsedDirectives,
            parsedParameters,
            parsedOptions,
            parsedEnvironmentVariables
        );
    }
}
