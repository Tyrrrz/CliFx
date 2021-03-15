using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Utils.Extensions;

namespace CliFx.Schema
{
    internal partial class ApplicationSchema
    {
        public IReadOnlyList<CommandSchema> Commands { get; }

        public ApplicationSchema(IReadOnlyList<CommandSchema> commands)
        {
            Commands = commands;
        }

        public IReadOnlyList<string> GetCommandNames() => Commands
            .Select(c => c.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToArray()!;

        public CommandSchema? TryFindDefaultCommand() =>
            Commands.FirstOrDefault(c => c.IsDefault);

        public CommandSchema? TryFindCommand(string? commandName) =>
            Commands.FirstOrDefault(c => c.MatchesName(commandName));

        private IReadOnlyList<CommandSchema> GetDescendantCommands(
            IReadOnlyList<CommandSchema> potentialParentCommands,
            string? parentCommandName) =>
            potentialParentCommands
                // Default commands can't be children of anything
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                // Command can't be its own child
                .Where(c => !c.MatchesName(parentCommandName))
                .Where(c =>
                    string.IsNullOrWhiteSpace(parentCommandName) ||
                    c.Name!.StartsWith(parentCommandName + ' ', StringComparison.OrdinalIgnoreCase))
                .ToArray();

        public IReadOnlyList<CommandSchema> GetDescendantCommands(string? parentCommandName) =>
            GetDescendantCommands(Commands, parentCommandName);

        public IReadOnlyList<CommandSchema> GetChildCommands(string? parentCommandName)
        {
            var descendants = GetDescendantCommands(parentCommandName);

            // Filter out descendants of descendants, leave only children
            var result = new List<CommandSchema>(descendants);

            foreach (var descendant in descendants)
            {
                var descendantDescendants = GetDescendantCommands(descendants, descendant.Name);
                result.RemoveRange(descendantDescendants);
            }

            return result;
        }
    }

    internal partial class ApplicationSchema
    {
        public static ApplicationSchema Resolve(IReadOnlyList<Type> commandTypes) => new(
            commandTypes.Select(CommandSchema.Resolve).ToArray()
        );
    }
}