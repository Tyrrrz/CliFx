using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Exceptions;

namespace CliFx.Domain
{
    /// <summary>
    /// Stores all commands in application.
    /// </summary>
    public partial class RootSchema
    {
        /// <summary>
        /// List of defined commands in application.
        /// </summary>
        public IReadOnlyDictionary<string, CommandSchema> Commands { get; }

        /// <summary>
        /// Default command (null if no default command).
        /// </summary>
        public CommandSchema? DefaultCommand { get; }

        private HashSet<string>? _commandNamesHashSet;

        /// <summary>
        /// Initializes an instance of <see cref="RootSchema"/>.
        /// </summary>
        public RootSchema(IReadOnlyDictionary<string, CommandSchema> commands, CommandSchema? defaultCommand)
        {
            Commands = commands;
            DefaultCommand = defaultCommand;
        }

        /// <summary>
        /// Returns collection of commands names.
        /// </summary>
        public ISet<string> GetCommandNames()
        {
            _commandNamesHashSet ??= Commands.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

            return _commandNamesHashSet;
        }

        /// <summary>
        /// Finds command schema by name.
        /// </summary>
        public CommandSchema? TryFindCommand(string? commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                return DefaultCommand;

            Commands.TryGetValue(commandName, out var value);

            return value;
        }

        private IEnumerable<CommandSchema> GetDescendantCommands(IEnumerable<CommandSchema> potentialParentCommands, string? parentCommandName)
        {
            return potentialParentCommands.Where(c => string.IsNullOrWhiteSpace(parentCommandName) ||
                                                 c.Name!.StartsWith(parentCommandName + ' ', StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<CommandSchema> GetDescendantCommands(string? parentCommandName)
        {
            return GetDescendantCommands(Commands.Values, parentCommandName).ToArray();
        }

        /// <summary>
        /// Finds all child commands of the parrent command by name.
        /// </summary>
        public IReadOnlyList<CommandSchema> GetChildCommands(string? parentCommandName)
        {
            var descendants = GetDescendantCommands(Commands.Values, parentCommandName);

            // Filter out descendants of descendants, leave only children
            var result = new List<CommandSchema>(descendants);

            foreach (var descendant in descendants)
            {
                var descendantDescendants = GetDescendantCommands(descendants, descendant.Name).ToHashSet();
                result.RemoveAll(t => descendantDescendants.Contains(t));
            }

            return result;
        }
    }

    public partial class RootSchema
    {
        private static void ValidateParameters(CommandSchema command)
        {
            var duplicateOrderGroup = command.Parameters
                .GroupBy(a => a.Order)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateOrderGroup != null)
            {
                throw CliFxException.ParametersWithSameOrder(
                    command,
                    duplicateOrderGroup.Key,
                    duplicateOrderGroup.ToArray()
                );
            }

            var duplicateNameGroup = command.Parameters
                .Where(a => !string.IsNullOrWhiteSpace(a.Name))
                .GroupBy(a => a.Name!, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup != null)
            {
                throw CliFxException.ParametersWithSameName(
                    command,
                    duplicateNameGroup.Key,
                    duplicateNameGroup.ToArray()
                );
            }

            var nonScalarParameters = command.Parameters
                                             .Where(p => !p.IsScalar)
                                             .ToArray();

            if (nonScalarParameters.Length > 1)
            {
                throw CliFxException.TooManyNonScalarParameters(
                    command,
                    nonScalarParameters
                );
            }

            var nonLastNonScalarParameter = command.Parameters
                .OrderByDescending(a => a.Order)
                .Skip(1)
                .LastOrDefault(p => !p.IsScalar);

            if (nonLastNonScalarParameter != null)
            {
                throw CliFxException.NonLastNonScalarParameter(
                    command,
                    nonLastNonScalarParameter
                );
            }
        }

        private static void ValidateOptions(CommandSchema command)
        {
            IEnumerable<CommandOptionSchema> noNameGroup = command.Options
                                                                  .Where(o => o.ShortName == null && string.IsNullOrWhiteSpace(o.Name));

            if (noNameGroup.Any())
            {
                throw CliFxException.OptionsWithNoName(
                    command,
                    noNameGroup.ToArray()
                );
            }

            var invalidLengthNameGroup = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .Where(o => o.Name!.Length <= 1)
                .ToArray();

            if (invalidLengthNameGroup.Any())
            {
                throw CliFxException.OptionsWithInvalidLengthName(
                    command,
                    invalidLengthNameGroup
                );
            }

            var duplicateNameGroup = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .GroupBy(o => o.Name!, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup != null)
            {
                throw CliFxException.OptionsWithSameName(
                    command,
                    duplicateNameGroup.Key,
                    duplicateNameGroup.ToArray()
                );
            }

            var duplicateShortNameGroup = command.Options
                .Where(o => o.ShortName != null)
                .GroupBy(o => o.ShortName!.Value)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateShortNameGroup != null)
            {
                throw CliFxException.OptionsWithSameShortName(
                    command,
                    duplicateShortNameGroup.Key,
                    duplicateShortNameGroup.ToArray()
                );
            }

            var duplicateEnvironmentVariableNameGroup = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.EnvironmentVariableName))
                .GroupBy(o => o.EnvironmentVariableName!, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateEnvironmentVariableNameGroup != null)
            {
                throw CliFxException.OptionsWithSameEnvironmentVariableName(
                    command,
                    duplicateEnvironmentVariableNameGroup.Key,
                    duplicateEnvironmentVariableNameGroup.ToArray()
                );
            }
        }

        public static RootSchema Resolve(IReadOnlyList<Type> commandTypes)
        {
            var commands = new Dictionary<string, CommandSchema>();
            var invalidCommands = new List<CommandSchema>();
            CommandSchema? defaultCommand = null;

            foreach (var commandType in commandTypes)
            {
                var command = CommandSchema.TryResolve(commandType) ?? throw CliFxException.InvalidCommandType(commandType);

                ValidateParameters(command);
                ValidateOptions(command);

                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    defaultCommand = defaultCommand is null ? command : throw CliFxException.TooManyDefaultCommands();

                    continue;
                }

                if (!commands.TryAdd(command.Name, command))
                    invalidCommands.Add(command);
            }

            if (commands.Count == 0 && defaultCommand is null)
                throw CliFxException.NoCommandsDefined();

            if (invalidCommands.Count > 0)
            {
                var duplicateNameGroup = invalidCommands.Union(commands.Values)
                                                        .GroupBy(c => c.Name!, StringComparer.OrdinalIgnoreCase)
                                                        .FirstOrDefault();

                throw CliFxException.CommandsWithSameName(duplicateNameGroup.Key, duplicateNameGroup.ToArray());
            }

            return new RootSchema(commands, defaultCommand);
        }
    }
}