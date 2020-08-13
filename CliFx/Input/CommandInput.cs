namespace CliFx.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using CliFx.Internal.Extensions;

    /// <summary>
    /// Provides a command parser and command class represention.
    /// <remarks>
    /// Command schema is `{directives} {command name} {parameters} {options}`.
    /// </remarks>
    /// </summary>
    public partial class CommandInput
    {
        /// <summary>
        /// Whether interactive directive [interactive] is specified.
        /// </summary>
        public bool IsInteractiveDirectiveSpecified { get; }

        /// <summary>
        /// Command direcitves list without special [interactive] directive.
        /// </summary>
        public IReadOnlyList<CommandDirectiveInput> Directives { get; }

        /// <summary>
        /// Command name. Null or empty/whitespace if default command.
        /// </summary>
        public string? CommandName { get; }

        /// <summary>
        /// Command parameters list.
        /// </summary>
        public IReadOnlyList<CommandParameterInput> Parameters { get; }

        /// <summary>
        /// Command options list.
        /// </summary>
        public IReadOnlyList<CommandOptionInput> Options { get; }

        /// <summary>
        /// Whether command has help option specified (--help|-h).
        /// </summary>
        public bool IsHelpOptionSpecified => Options.Any(o => o.IsHelpOption);

        /// <summary>
        /// Whether command has version option specified (--version).
        /// </summary>
        public bool IsVersionOptionSpecified => Options.Any(o => o.IsVersionOption);

        /// <summary>
        /// Whether command input is default command or empty (no command name, no options, no parameters, and no directives other than [interactive]_.
        /// </summary>
        public bool IsDefaultCommandOrEmpty { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(bool isInteractiveDirectiveSpecified,
                            IReadOnlyList<CommandDirectiveInput> directives,
                            string? commandName,
                            IReadOnlyList<CommandParameterInput> parameters,
                            IReadOnlyList<CommandOptionInput> options)
        {
            IsInteractiveDirectiveSpecified = isInteractiveDirectiveSpecified;
            Directives = directives;
            CommandName = commandName;
            Parameters = parameters;
            Options = options;

            IsDefaultCommandOrEmpty = Options.Count == 0 &&
                                      Parameters.Count == 0 &&
                                      Directives.Count == 0 &&
                                      string.IsNullOrWhiteSpace(CommandName);
        }

        /// <summary>
        /// Whether command has a directive.
        /// </summary>
        public bool HasDirective(string directive)
        {
            string v = directive.Trim('[', ']')
                                .ToLower();

            return Directives.Where(x => x.Name == v)
                             .Any();
        }

        /// <summary>
        /// Whether command has any of given directives.
        /// </summary>
        internal bool HasAnyOfDirectives(string[] directives)
        {
            foreach (string directive in directives)
            {
                if (HasDirective(directive))
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var buffer = new StringBuilder();

            foreach (CommandDirectiveInput directive in Directives)
            {
                buffer.AppendIfNotEmpty(' ')
                      .Append(directive);
            }

            if (!string.IsNullOrWhiteSpace(CommandName))
            {
                buffer.AppendIfNotEmpty(' ')
                      .Append(CommandName);
            }

            foreach (CommandParameterInput parameter in Parameters)
            {
                buffer.AppendIfNotEmpty(' ')
                      .Append(parameter);
            }

            foreach (CommandOptionInput option in Options)
            {
                buffer.AppendIfNotEmpty(' ')
                      .Append(option);
            }

            return buffer.ToString();
        }
    }

    public partial class CommandInput
    {
        private static IReadOnlyList<CommandDirectiveInput> ParseDirectives(IReadOnlyList<string> commandLineArguments,
                                                                            ref int index,
                                                                            out bool isInteractiveDirectiveSpecified)
        {
            isInteractiveDirectiveSpecified = false;
            var result = new List<CommandDirectiveInput>();

            for (; index < commandLineArguments.Count; index++)
            {
                string argument = commandLineArguments[index];

                if (!argument.StartsWith('[') || !argument.EndsWith(']'))
                    break;

                string name = argument.Substring(1, argument.Length - 2);

                if (name == BuiltInDirectives.Interactive)
                    isInteractiveDirectiveSpecified = true;
                else
                    result.Add(new CommandDirectiveInput(name));
            }

            return result;
        }

        private static string? ParseCommandName(IReadOnlyList<string> commandLineArguments,
                                                ISet<string> commandNames,
                                                ref int index)
        {
            var buffer = new List<string>();

            string? commandName = null;
            int lastIndex = index;

            // We need to look ahead to see if we can match as many consecutive arguments to a command name as possible
            for (int i = index; i < commandLineArguments.Count; i++)
            {
                string argument = commandLineArguments[i];
                buffer.Add(argument);

                string potentialCommandName = buffer.JoinToString(" ");

                if (commandNames.Contains(potentialCommandName))
                {
                    commandName = potentialCommandName;
                    lastIndex = i;
                }
            }

            // Update the index only if command name was found in the arguments
            if (!string.IsNullOrWhiteSpace(commandName))
                index = lastIndex + 1;

            return commandName;
        }

        private static IReadOnlyList<CommandParameterInput> ParseParameters(IReadOnlyList<string> commandLineArguments,
                                                                            ref int index)
        {
            var result = new List<CommandParameterInput>();

            for (; index < commandLineArguments.Count; index++)
            {
                string argument = commandLineArguments[index];

                if (argument.StartsWith('-'))
                    break;

                result.Add(new CommandParameterInput(argument));
            }

            return result;
        }

        private static IReadOnlyList<CommandOptionInput> ParseOptions(IReadOnlyList<string> commandLineArguments,
                                                                      ref int index)
        {
            var result = new List<CommandOptionInput>();

            string? currentOptionAlias = null;
            var currentOptionValues = new List<string>();

            for (; index < commandLineArguments.Count; index++)
            {
                string argument = commandLineArguments[index];

                // Name
                if (argument.StartsWith("--", StringComparison.Ordinal))
                {
                    // Flush previous
                    if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                        result.Add(new CommandOptionInput(currentOptionAlias, currentOptionValues));

                    currentOptionAlias = argument.Substring(2);
                    currentOptionValues = new List<string>();
                }
                // Short name
                else if (argument.StartsWith('-'))
                {
                    foreach (var alias in argument.Substring(1))
                    {
                        // Flush previous
                        if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                            result.Add(new CommandOptionInput(currentOptionAlias, currentOptionValues));

                        currentOptionAlias = alias.AsString();
                        currentOptionValues = new List<string>();
                    }
                }
                // Value
                else if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                {
                    currentOptionValues.Add(argument);
                }
            }

            // Flush last option
            if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                result.Add(new CommandOptionInput(currentOptionAlias, currentOptionValues));

            return result;
        }

        internal static CommandInput Parse(IReadOnlyList<string> commandLineArguments,
                                           ISet<string> availableCommandNamesSet)
        {
            int index = 0;

            IReadOnlyList<CommandDirectiveInput> directives = ParseDirectives(
                commandLineArguments,
                ref index,
                out bool isInteractiveDirectiveSpecified
            );

            string? commandName = ParseCommandName(
                commandLineArguments,
                availableCommandNamesSet,
                ref index
            );

            IReadOnlyList<CommandParameterInput> parameters = ParseParameters(
                commandLineArguments,
                ref index
            );

            IReadOnlyList<CommandOptionInput> options = ParseOptions(
                commandLineArguments,
                ref index
            );

            return new CommandInput(isInteractiveDirectiveSpecified, directives, commandName, parameters, options);
        }
    }

    public partial class CommandInput
    {
        internal static CommandInput Empty { get; } = new CommandInput(
            false,
            Array.Empty<CommandDirectiveInput>(),
            null,
            Array.Empty<CommandParameterInput>(),
            Array.Empty<CommandOptionInput>()
        );
    }
}