using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Exceptions;

namespace CliFx.Domain
{
    /// <summary>
    /// Stores all schemas of commands and directives in the application.
    /// </summary>
    public partial class RootSchema
    {
        /// <summary>
        /// List of defined directives in the application.
        /// </summary>
        public IReadOnlyDictionary<string, DirectiveSchema> Directives { get; }

        private HashSet<string>? _directiveNamesHashSet;

        /// <summary>
        /// List of defined commands in the application.
        /// </summary>
        public IReadOnlyDictionary<string, CommandSchema> Commands { get; }

        private HashSet<string>? _commandNamesHashSet;

        /// <summary>
        /// Default command (null if no default command).
        /// </summary>
        public CommandSchema? DefaultCommand { get; }

        /// <summary>
        /// Initializes an instance of <see cref="RootSchema"/>.
        /// </summary>
        internal RootSchema(IReadOnlyDictionary<string, DirectiveSchema> directives,
                            IReadOnlyDictionary<string, CommandSchema> commands,
                            CommandSchema? defaultCommand)
        {
            Directives = directives;
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
        /// Returns collection of directives names.
        /// </summary>
        public ISet<string> GetDirectivesNames()
        {
            _directiveNamesHashSet ??= Directives.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

            return _directiveNamesHashSet;
        }

        /// <summary>
        /// Finds command schema by name.
        /// </summary>
        public CommandSchema? TryFindCommand(string? commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                return DefaultCommand;

            Commands.TryGetValue(commandName, out CommandSchema value);

            return value;
        }

        /// <summary>
        /// Finds command schema by name.
        /// </summary>
        public DirectiveSchema? TryFindDirective(string directiveName)
        {
            if (string.IsNullOrWhiteSpace(directiveName))
                return null;

            Directives.TryGetValue(directiveName, out DirectiveSchema value);

            return value;
        }

        private IEnumerable<CommandSchema> GetDescendantCommands(IEnumerable<CommandSchema> potentialParentCommands, string? parentCommandName)
        {
            return potentialParentCommands.Where(c => string.IsNullOrWhiteSpace(parentCommandName) ||
                                                 c.Name!.StartsWith(parentCommandName + ' ', StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Finds all descendant commands of the parrent command by name.
        /// </summary>
        public IReadOnlyList<CommandSchema> GetDescendantCommands(string? parentCommandName)
        {
            return GetDescendantCommands(Commands.Values, parentCommandName).ToArray();
        }

        /// <summary>
        /// Finds all child commands of the parrent command by name.
        /// </summary>
        public IReadOnlyList<CommandSchema> GetChildCommands(string? parentCommandName)
        {
            IEnumerable<CommandSchema> descendants = GetDescendantCommands(Commands.Values, parentCommandName);

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
        /// <summary>
        /// Resolves the root schema.
        /// </summary>
        internal static RootSchema Resolve(IReadOnlyList<Type> commandTypes, IReadOnlyList<Type> directiveTypes)
        {
            //Resolve commands
            var commands = new Dictionary<string, CommandSchema>();
            var invalidCommands = new List<CommandSchema>();
            CommandSchema? defaultCommand = null;

            foreach (Type commandType in commandTypes)
            {
                CommandSchema command = CommandSchema.TryResolve(commandType) ?? throw CliFxException.InvalidCommandType(commandType);

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

            //Resolve directives
            var directives = new Dictionary<string, DirectiveSchema>();
            var invalidDirectives = new List<DirectiveSchema>();

            foreach (var directiveType in directiveTypes)
            {
                DirectiveSchema directive = DirectiveSchema.TryResolve(directiveType) ?? throw CliFxException.InvalidDirectiveType(directiveType);

                if (!directives.TryAdd(directive.Name, directive))
                    invalidDirectives.Add(directive);
            }

            if (invalidDirectives.Count > 0)
            {
                var duplicateNameGroup = invalidDirectives.Union(directives.Values)
                                                          .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                                                          .FirstOrDefault();

                throw CliFxException.DirectiveWithSameName(duplicateNameGroup.Key, duplicateNameGroup.ToArray());
            }

            return new RootSchema(directives, commands, defaultCommand);
        }
    }
}