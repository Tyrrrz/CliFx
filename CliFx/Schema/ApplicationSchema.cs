using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

internal partial class ApplicationSchema(IReadOnlyList<CommandSchema> commands)
{
    public IReadOnlyList<CommandSchema> Commands { get; } = commands;

    public IReadOnlyList<string> GetCommandNames() =>
        Commands.Select(c => c.Name).WhereNotNullOrWhiteSpace().ToArray();

    public CommandSchema? TryFindDefaultCommand() => Commands.FirstOrDefault(c => c.IsDefault);

    public CommandSchema? TryFindCommand(string commandName) =>
        Commands.FirstOrDefault(c => c.MatchesName(commandName));

    private IReadOnlyList<CommandSchema> GetDescendantCommands(
        IReadOnlyList<CommandSchema> potentialParentCommandSchemas,
        string? parentCommandName
    )
    {
        var result = new List<CommandSchema>();

        foreach (var potentialParentCommandSchema in potentialParentCommandSchemas)
        {
            // Default commands can't be descendant of anything
            if (string.IsNullOrWhiteSpace(potentialParentCommandSchema.Name))
                continue;

            // Command can't be its own descendant
            if (potentialParentCommandSchema.MatchesName(parentCommandName))
                continue;

            var isDescendant =
                // Every command is a descendant of the default command
                string.IsNullOrWhiteSpace(parentCommandName)
                ||
                // Otherwise a command is a descendant if it starts with the same name segments
                potentialParentCommandSchema.Name.StartsWith(
                    parentCommandName + ' ',
                    StringComparison.OrdinalIgnoreCase
                );

            if (isDescendant)
                result.Add(potentialParentCommandSchema);
        }

        return result;
    }

    public IReadOnlyList<CommandSchema> GetDescendantCommands(string? parentCommandName) =>
        GetDescendantCommands(Commands, parentCommandName);

    public IReadOnlyList<CommandSchema> GetChildCommands(string? parentCommandName)
    {
        var descendants = GetDescendantCommands(parentCommandName);

        var result = descendants.ToList();

        // Filter out descendants of descendants, leave only direct children
        foreach (var descendant in descendants)
        {
            result.RemoveRange(GetDescendantCommands(descendants, descendant.Name));
        }

        return result;
    }
}

internal partial class ApplicationSchema
{
    public static ApplicationSchema Resolve(IReadOnlyList<Type> commandTypes) =>
        new(commandTypes.Select(CommandSchema.Resolve).ToArray());
}
