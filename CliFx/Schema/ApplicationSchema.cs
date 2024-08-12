using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

/// <summary>
/// Describes the structure of a command-line application.
/// </summary>
public class ApplicationSchema(IReadOnlyList<CommandSchema> commands)
{
    /// <summary>
    /// Commands defined in the application.
    /// </summary>
    public IReadOnlyList<CommandSchema> Commands { get; } = commands;

    internal IReadOnlyList<string> GetCommandNames() =>
        Commands.Select(c => c.Name).WhereNotNullOrWhiteSpace().ToArray();

    internal CommandSchema? TryFindDefaultCommand() => Commands.FirstOrDefault(c => c.IsDefault);

    internal CommandSchema? TryFindCommand(string commandName) =>
        Commands.FirstOrDefault(c => c.MatchesName(commandName));

    private IReadOnlyList<CommandSchema> GetDescendantCommands(
        IReadOnlyList<CommandSchema> potentialDescendantCommands,
        string? parentCommandName
    )
    {
        var result = new List<CommandSchema>();

        foreach (var potentialDescendantCommand in potentialDescendantCommands)
        {
            // Default commands can't be descendants of anything
            if (string.IsNullOrWhiteSpace(potentialDescendantCommand.Name))
                continue;

            // Command can't be its own descendant
            if (potentialDescendantCommand.MatchesName(parentCommandName))
                continue;

            var isDescendant =
                // Every command is a descendant of the default command
                string.IsNullOrWhiteSpace(parentCommandName)
                ||
                // Otherwise a command is a descendant if it starts with the same name segments
                potentialDescendantCommand.Name.StartsWith(
                    parentCommandName + ' ',
                    StringComparison.OrdinalIgnoreCase
                );

            if (isDescendant)
                result.Add(potentialDescendantCommand);
        }

        return result;
    }

    internal IReadOnlyList<CommandSchema> GetDescendantCommands(string? parentCommandName) =>
        GetDescendantCommands(Commands, parentCommandName);

    internal IReadOnlyList<CommandSchema> GetChildCommands(string? parentCommandName)
    {
        var descendants = GetDescendantCommands(parentCommandName);

        var result = descendants.ToList();

        // Filter out descendants of descendants, leave only direct children
        foreach (var descendant in descendants)
            result.RemoveRange(GetDescendantCommands(descendants, descendant.Name));

        return result;
    }
}
