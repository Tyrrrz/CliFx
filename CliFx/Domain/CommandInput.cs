using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CliFx.Internal.Extensions;

namespace CliFx.Domain
{
    internal partial class CommandInput
    {
        public IReadOnlyList<CommandDirectiveInput> Directives { get; }

        public string? CommandName { get; }

        public IReadOnlyList<CommandParameterInput> Parameters { get; }

        public IReadOnlyList<CommandOptionInput> Options { get; }

        public bool IsDebugDirectiveSpecified => Directives.Any(d => d.IsDebugDirective);

        public bool IsPreviewDirectiveSpecified => Directives.Any(d => d.IsPreviewDirective);

        public bool IsHelpOptionSpecified => Options.Any(o => o.IsHelpOption);

        public bool IsVersionOptionSpecified => Options.Any(o => o.IsVersionOption);

        public CommandInput(
            IReadOnlyList<CommandDirectiveInput> directives,
            string? commandName,
            IReadOnlyList<CommandParameterInput> parameters,
            IReadOnlyList<CommandOptionInput> options)
        {
            Directives = directives;
            CommandName = commandName;
            Parameters = parameters;
            Options = options;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var buffer = new StringBuilder();

            foreach (var directive in Directives)
            {
                buffer
                    .AppendIfNotEmpty(' ')
                    .Append(directive);
            }

            if (!string.IsNullOrWhiteSpace(CommandName))
            {
                buffer
                    .AppendIfNotEmpty(' ')
                    .Append(CommandName);
            }

            foreach (var parameter in Parameters)
            {
                buffer
                    .AppendIfNotEmpty(' ')
                    .Append(parameter);
            }

            foreach (var option in Options)
            {
                buffer
                    .AppendIfNotEmpty(' ')
                    .Append(option);
            }

            return buffer.ToString();
        }
    }

    internal partial class CommandInput
    {
        private static IReadOnlyList<CommandDirectiveInput> ParseDirectives(
            IReadOnlyList<string> commandLineArguments,
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

        private static string? ParseCommandName(
            IReadOnlyList<string> commandLineArguments,
            ISet<string> commandNames,
            ref int index)
        {
            var buffer = new List<string>();

            var commandName = default(string?);
            var lastIndex = index;

            // We need to look ahead to see if we can match as many consecutive arguments to a command name as possible
            for (var i = index; i < commandLineArguments.Count; i++)
            {
                var argument = commandLineArguments[i];
                buffer.Add(argument);

                var potentialCommandName = buffer.JoinToString(" ");

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

        private static IReadOnlyList<CommandParameterInput> ParseParameters(
            IReadOnlyList<string> commandLineArguments,
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

        private static IReadOnlyList<CommandOptionInput> ParseOptions(
            IReadOnlyList<string> commandLineArguments,
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

        public static CommandInput Parse(IReadOnlyList<string> commandLineArguments, IReadOnlyList<string> availableCommandNames)
        {
            var availableCommandNamesSet = availableCommandNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var index = 0;

            var directives = ParseDirectives(
                commandLineArguments,
                ref index
            );

            var commandName = ParseCommandName(
                commandLineArguments,
                availableCommandNamesSet,
                ref index
            );

            var parameters = ParseParameters(
                commandLineArguments,
                ref index
            );

            var options = ParseOptions(
                commandLineArguments,
                ref index
            );

            return new CommandInput(directives, commandName, parameters, options);
        }
    }

    internal partial class CommandInput
    {
        public static CommandInput Empty { get; } = new CommandInput(
            Array.Empty<CommandDirectiveInput>(),
            null,
            Array.Empty<CommandParameterInput>(),
            Array.Empty<CommandOptionInput>()
        );
    }
}