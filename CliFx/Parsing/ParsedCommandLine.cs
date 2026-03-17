using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Utils.Extensions;

namespace CliFx.Parsing;

internal partial class ParsedCommandLine(
    string? commandName,
    IReadOnlyList<ParsedDirective> directives,
    IReadOnlyList<ParsedPositionalArgument> positionalArguments,
    IReadOnlyList<ParsedOption> options,
    IReadOnlyList<ParsedEnvironmentVariable> environmentVariables
)
{
    public string? CommandName { get; } = commandName;

    public IReadOnlyList<ParsedDirective> Directives { get; } = directives;

    public IReadOnlyList<ParsedPositionalArgument> PositionalArguments { get; } =
        positionalArguments;

    public IReadOnlyList<ParsedOption> Options { get; } = options;

    public IReadOnlyList<ParsedEnvironmentVariable> EnvironmentVariables { get; } =
        environmentVariables;

    public bool HasArguments =>
        !string.IsNullOrWhiteSpace(CommandName)
        || Directives.Any()
        || PositionalArguments.Any()
        || Options.Any();

    public bool IsDebugDirectiveSpecified => Directives.Any(d => d.IsDebugDirective);

    public bool IsPreviewDirectiveSpecified => Directives.Any(d => d.IsPreviewDirective);
}

internal partial class ParsedCommandLine
{
    private static IReadOnlyList<ParsedDirective> ParseDirectives(
        IReadOnlyList<string> commandLineArguments,
        ref int index
    )
    {
        var result = new List<ParsedDirective>();

        // Consume all consecutive directive arguments
        for (; index < commandLineArguments.Count; index++)
        {
            var argument = commandLineArguments[index];

            // Break on the first non-directive argument
            if (!argument.StartsWith('[') || !argument.EndsWith(']'))
                break;

            var directiveName = argument.Substring(1, argument.Length - 2);
            result.Add(new ParsedDirective(directiveName));
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

            var potentialCommandName = string.Join(" ", potentialCommandNameComponents);
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

    private static IReadOnlyList<ParsedPositionalArgument> ParsePositionalArguments(
        IReadOnlyList<string> commandLineArguments,
        ref int index
    )
    {
        var result = new List<ParsedPositionalArgument>();

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

            result.Add(new ParsedPositionalArgument(argument));
        }

        return result;
    }

    private static IReadOnlyList<ParsedOption> ParseOptions(
        IReadOnlyList<string> commandLineArguments,
        ref int index
    )
    {
        var result = new List<ParsedOption>();

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
                    result.Add(new ParsedOption(lastOptionIdentifier, lastOptionValues));

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
                        result.Add(new ParsedOption(lastOptionIdentifier, lastOptionValues));

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
            result.Add(new ParsedOption(lastOptionIdentifier, lastOptionValues));

        return result;
    }

    public static ParsedCommandLine Parse(
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

        var parsedPositionalArguments = ParsePositionalArguments(commandLineArguments, ref index);

        var parsedOptions = ParseOptions(commandLineArguments, ref index);

        var parsedEnvironmentVariables = environmentVariables
            .Select(kvp => new ParsedEnvironmentVariable(kvp.Key, kvp.Value))
            .ToArray();

        return new ParsedCommandLine(
            parsedCommandName,
            parsedDirectives,
            parsedPositionalArguments,
            parsedOptions,
            parsedEnvironmentVariables
        );
    }
}
