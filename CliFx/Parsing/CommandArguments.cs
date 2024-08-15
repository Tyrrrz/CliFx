using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Utils.Extensions;

namespace CliFx.Parsing;

/// <summary>
/// Command-line arguments provided by the user, parsed into their semantic form.
/// </summary>
public partial class CommandArguments(
    string? commandName,
    IReadOnlyList<CommandDirectiveToken> directives,
    IReadOnlyList<CommandParameterToken> parameters,
    IReadOnlyList<CommandOptionToken> options
)
{
    /// <summary>
    /// Name of the requested command.
    /// </summary>
    public string? CommandName { get; } = commandName;

    /// <summary>
    /// Provided directives.
    /// </summary>
    public IReadOnlyList<CommandDirectiveToken> Directives { get; } = directives;

    /// <summary>
    /// Provided parameters.
    /// </summary>
    public IReadOnlyList<CommandParameterToken> Parameters { get; } = parameters;

    /// <summary>
    /// Provided options.
    /// </summary>
    public IReadOnlyList<CommandOptionToken> Options { get; } = options;

    internal bool IsDebugDirectiveSpecified => Directives.Any(d => d.IsDebugDirective);

    internal bool IsPreviewDirectiveSpecified => Directives.Any(d => d.IsPreviewDirective);
}

public partial class CommandArguments
{
    private static IReadOnlyList<CommandDirectiveToken> ParseDirectives(
        IReadOnlyList<string> rawArguments,
        ref int index
    )
    {
        var result = new List<CommandDirectiveToken>();

        // Consume all consecutive directive arguments
        for (; index < rawArguments.Count; index++)
        {
            var rawArgument = rawArguments[index];

            // Break on the first non-directive argument
            if (!rawArgument.StartsWith('[') || !rawArgument.EndsWith(']'))
                break;

            var directiveName = rawArgument.Substring(1, rawArgument.Length - 2);
            result.Add(new CommandDirectiveToken(directiveName));
        }

        return result;
    }

    private static string? ParseCommandName(
        IReadOnlyList<string> rawArguments,
        ISet<string> commandNames,
        ref int index
    )
    {
        var potentialCommandNameComponents = new List<string>();
        var commandName = default(string?);

        var lastIndex = index;

        // Append arguments to a buffer until we find the longest sequence that represents
        // a valid command name.
        for (var i = index; i < rawArguments.Count; i++)
        {
            var rawArgument = rawArguments[i];

            potentialCommandNameComponents.Add(rawArgument);

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

    private static IReadOnlyList<CommandParameterToken> ParseParameters(
        IReadOnlyList<string> rawArguments,
        ref int index
    )
    {
        var result = new List<CommandParameterToken>();

        // Consume all arguments until the first option identifier
        for (; index < rawArguments.Count; index++)
        {
            var rawArgument = rawArguments[index];

            var isOptionIdentifier =
                // Name
                rawArgument.StartsWith("--", StringComparison.Ordinal)
                    && rawArgument.Length > 2
                    && char.IsLetter(rawArgument[2])
                ||
                // Short name
                rawArgument.StartsWith('-')
                    && rawArgument.Length > 1
                    && char.IsLetter(rawArgument[1]);

            // Break on the first option identifier
            if (isOptionIdentifier)
                break;

            result.Add(new CommandParameterToken(index, rawArgument));
        }

        return result;
    }

    private static IReadOnlyList<CommandOptionToken> ParseOptions(
        IReadOnlyList<string> rawArguments,
        ref int index
    )
    {
        var result = new List<CommandOptionToken>();

        var lastOptionIdentifier = default(string?);
        var lastOptionValues = new List<string>();

        // Consume and group all remaining arguments into options
        for (; index < rawArguments.Count; index++)
        {
            var rawArgument = rawArguments[index];

            // Name
            if (
                rawArgument.StartsWith("--", StringComparison.Ordinal)
                && rawArgument.Length > 2
                && char.IsLetter(rawArgument[2])
            )
            {
                // Flush previous
                if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
                    result.Add(new CommandOptionToken(lastOptionIdentifier, lastOptionValues));

                lastOptionIdentifier = rawArgument[2..];
                lastOptionValues = [];
            }
            // Short name
            else if (
                rawArgument.StartsWith('-')
                && rawArgument.Length > 1
                && char.IsLetter(rawArgument[1])
            )
            {
                foreach (var identifier in rawArgument[1..])
                {
                    // Flush previous
                    if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
                        result.Add(new CommandOptionToken(lastOptionIdentifier, lastOptionValues));

                    lastOptionIdentifier = identifier.AsString();
                    lastOptionValues = [];
                }
            }
            // Value
            else if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
            {
                lastOptionValues.Add(rawArgument);
            }
        }

        // Flush the last option
        if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
            result.Add(new CommandOptionToken(lastOptionIdentifier, lastOptionValues));

        return result;
    }

    internal static CommandArguments Parse(
        IReadOnlyList<string> rawArguments,
        IReadOnlyList<string> availableCommandNames
    )
    {
        var index = 0;

        var directives = ParseDirectives(rawArguments, ref index);

        var commandName = ParseCommandName(
            rawArguments,
            availableCommandNames.ToHashSet(StringComparer.OrdinalIgnoreCase),
            ref index
        );

        var parameters = ParseParameters(rawArguments, ref index);

        var options = ParseOptions(rawArguments, ref index);

        return new CommandArguments(commandName, directives, parameters, options);
    }
}
