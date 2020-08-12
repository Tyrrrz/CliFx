using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Internal.Extensions;

namespace CliFx.Domain.Input
{
    /// <summary>
    /// Provides a command parser and command class represention.
    /// <remarks>
    /// Command schema is `{directives} {command name} {parameters} {options}`.
    /// </remarks>
    /// </summary>
    public partial class CommandInput
    {
        /// <summary>
        /// Command direcitves list.
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
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(IReadOnlyList<CommandDirectiveInput> directives,
                            string? commandName,
                            IReadOnlyList<CommandParameterInput> parameters,
                            IReadOnlyList<CommandOptionInput> options)
        {
            Directives = directives;
            CommandName = commandName;
            Parameters = parameters;
            Options = options;
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
            foreach (var directive in directives)
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

            foreach (var directive in Directives)
            {
                buffer.AppendIfNotEmpty(' ')
                      .Append(directive);
            }

            if (!string.IsNullOrWhiteSpace(CommandName))
            {
                buffer.AppendIfNotEmpty(' ')
                      .Append(CommandName);
            }

            foreach (var parameter in Parameters)
            {
                buffer.AppendIfNotEmpty(' ')
                      .Append(parameter);
            }

            foreach (var option in Options)
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
                                                                            ref int index)
        {
            var result = new List<CommandDirectiveInput>();

            for (; index < commandLineArguments.Count; index++)
            {
                var argument = commandLineArguments[index];

                if (!argument.StartsWith('[') || !argument.EndsWith(']'))
                    break;

                var name = argument.Substring(1, argument.Length - 2);
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
                var argument = commandLineArguments[index];

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

            var currentOptionAlias = default(string?);
            var currentOptionValues = new List<string>();

            for (; index < commandLineArguments.Count; index++)
            {
                var argument = commandLineArguments[index];

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
                ref index
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

            return new CommandInput(directives, commandName, parameters, options);
        }
    }

    public partial class CommandInput
    {
        internal static CommandInput Empty { get; } = new CommandInput(
            Array.Empty<CommandDirectiveInput>(),
            null,
            Array.Empty<CommandParameterInput>(),
            Array.Empty<CommandOptionInput>()
        );
    }
}