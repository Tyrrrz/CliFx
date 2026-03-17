using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Utils.Extensions;

namespace CliFx.Binding;

internal class ApplicationDescriptor(IReadOnlyList<CommandDescriptor> commands)
{
    public IReadOnlyList<CommandDescriptor> Commands { get; } = commands;

    public IReadOnlyList<string> GetCommandNames() =>
        Commands.Select(c => c.Name).WhereNotNullOrWhiteSpace().ToArray();

    public CommandDescriptor? TryFindDefaultCommand() => Commands.FirstOrDefault(c => c.IsDefault);

    public CommandDescriptor? TryFindCommand(string commandName) =>
        Commands.FirstOrDefault(c => c.MatchesName(commandName));

    private IReadOnlyList<CommandDescriptor> GetDescendantCommands(
        IReadOnlyList<CommandDescriptor> potentialParentCommandDescriptors,
        string? parentCommandName
    )
    {
        var result = new List<CommandDescriptor>();

        foreach (var potentialParentCommandDescriptor in potentialParentCommandDescriptors)
        {
            if (string.IsNullOrWhiteSpace(potentialParentCommandDescriptor.Name))
                continue;

            if (potentialParentCommandDescriptor.MatchesName(parentCommandName))
                continue;

            var isDescendant =
                string.IsNullOrWhiteSpace(parentCommandName)
                || potentialParentCommandDescriptor.Name.StartsWith(
                    parentCommandName + ' ',
                    StringComparison.OrdinalIgnoreCase
                );

            if (isDescendant)
                result.Add(potentialParentCommandDescriptor);
        }

        return result;
    }

    public IReadOnlyList<CommandDescriptor> GetDescendantCommands(string? parentCommandName) =>
        GetDescendantCommands(Commands, parentCommandName);

    public IReadOnlyList<CommandDescriptor> GetChildCommands(string? parentCommandName)
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
