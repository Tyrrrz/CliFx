using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Utils.Extensions;

namespace CliFx.Input
{
    internal partial class CommandInput
    {
        public string? CommandName { get; }

        public IReadOnlyList<DirectiveInput> Directives { get; }

        public IReadOnlyList<ParameterInput> Parameters { get; }

        public IReadOnlyList<OptionInput> Options { get; }

        public IReadOnlyList<EnvironmentVariableInput> EnvironmentVariables { get; }

        public bool IsDebugDirectiveSpecified => Directives.Any(d => d.IsDebugDirective);

        public bool IsPreviewDirectiveSpecified => Directives.Any(d => d.IsPreviewDirective);

        public bool IsSuggestDirectiveSpecified => Directives.Any(d => d.IsSuggestDirective);

        public bool IsHelpOptionSpecified => Options.Any(o => o.IsHelpOption);

        public bool IsVersionOptionSpecified => Options.Any(o => o.IsVersionOption);

        public CommandInput(
            string? commandName,
            IReadOnlyList<DirectiveInput> directives,
            IReadOnlyList<ParameterInput> parameters,
            IReadOnlyList<OptionInput> options,
            IReadOnlyList<EnvironmentVariableInput> environmentVariables)
        {
            CommandName = commandName;
            Directives = directives;
            Parameters = parameters;
            Options = options;
            EnvironmentVariables = environmentVariables;
        }
    }

    internal partial class CommandInput
    {
        private static IReadOnlyList<DirectiveInput> ParseDirectives(
            IReadOnlyList<string> commandLineArguments,
            ref int index)
        {
            var result = new List<DirectiveInput>();

            // Consume all consecutive directive arguments
            for (; index < commandLineArguments.Count; index++)
            {
                var argument = commandLineArguments[index];

                // Break on the first non-directive argument
                if (!argument.StartsWith('[') || !argument.EndsWith(']'))
                    break;

                var directiveName = argument.Substring(1, argument.Length - 2);
                result.Add(new DirectiveInput(directiveName));
            }

            return result;
        }

        private static string? ParseCommandName(
            IReadOnlyList<string> commandLineArguments,
            ISet<string> commandNames,
            ref int index)
        {
            var potentialCommandNameComponents = new List<string>();
            var commandName = default(string?);

            var lastIndex = index;

            // Append arguments to a buffer until we find the longest sequence
            // that represents a valid command name.
            for (var i = index; i < commandLineArguments.Count; i++)
            {
                var argument = commandLineArguments[i];

                potentialCommandNameComponents.Add(argument);

                var potentialCommandName = potentialCommandNameComponents.JoinToString(" ");
                if (commandNames.Contains(potentialCommandName))
                {
                    // Record the position but continue the loop in case
                    // we find a longer (more specific) match.
                    commandName = potentialCommandName;
                    lastIndex = i;
                }
            }

            // Move the index to the position where the command name ended
            if (!string.IsNullOrWhiteSpace(commandName))
                index = lastIndex + 1;

            return commandName;
        }

        private static IReadOnlyList<ParameterInput> ParseParameters(
            IReadOnlyList<string> commandLineArguments,
            ref int index)
        {
            var result = new List<ParameterInput>();

            // Consume all arguments until first option identifier
            for (; index < commandLineArguments.Count; index++)
            {
                var argument = commandLineArguments[index];

                var isOptionIdentifier =
                    // Name
                    argument.StartsWith("--", StringComparison.Ordinal) &&
                    argument.Length > 2 &&
                    char.IsLetter(argument[2]) ||
                    // Short name
                    argument.StartsWith('-') &&
                    argument.Length > 1 &&
                    char.IsLetter(argument[1]);

                // Break on first option identifier
                if (isOptionIdentifier)
                    break;

                result.Add(new ParameterInput(argument));
            }

            return result;
        }

        private static IReadOnlyList<OptionInput> ParseOptions(
            IReadOnlyList<string> commandLineArguments,
            ref int index)
        {
            var result = new List<OptionInput>();

            var lastOptionIdentifier = default(string?);
            var lastOptionValues = new List<string>();

            // Consume and group all remaining arguments into options
            for (; index < commandLineArguments.Count; index++)
            {
                var argument = commandLineArguments[index];

                // Name
                if (argument.StartsWith("--", StringComparison.Ordinal) &&
                    argument.Length > 2 &&
                    char.IsLetter(argument[2]))
                {
                    // Flush previous
                    if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
                        result.Add(new OptionInput(lastOptionIdentifier, lastOptionValues));

                    lastOptionIdentifier = argument.Substring(2);
                    lastOptionValues = new List<string>();
                }
                // Short name
                else if (argument.StartsWith('-') &&
                         argument.Length > 1 &&
                         char.IsLetter(argument[1]))
                {
                    foreach (var alias in argument.Substring(1))
                    {
                        // Flush previous
                        if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
                            result.Add(new OptionInput(lastOptionIdentifier, lastOptionValues));

                        lastOptionIdentifier = alias.AsString();
                        lastOptionValues = new List<string>();
                    }
                }
                // Value
                else if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
                {
                    lastOptionValues.Add(argument);
                }
            }

            // Flush last option
            if (!string.IsNullOrWhiteSpace(lastOptionIdentifier))
                result.Add(new OptionInput(lastOptionIdentifier, lastOptionValues));

            return result;
        }

        public static CommandInput Parse(
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables,
            IReadOnlyList<string> availableCommandNames)
        {
            var index = 0;

            var parsedDirectives = ParseDirectives(
                commandLineArguments,
                ref index
            );

            var parsedCommandName = ParseCommandName(
                commandLineArguments,
                availableCommandNames.ToHashSet(StringComparer.OrdinalIgnoreCase),
                ref index
            );

            var parsedParameters = ParseParameters(
                commandLineArguments,
                ref index
            );

            var parsedOptions = ParseOptions(
                commandLineArguments,
                ref index
            );

            var parsedEnvironmentVariables = environmentVariables
                .Select(kvp => new EnvironmentVariableInput(kvp.Key, kvp.Value))
                .ToArray();

            return new CommandInput(
                parsedCommandName,
                parsedDirectives,
                parsedParameters,
                parsedOptions,
                parsedEnvironmentVariables
            );
        }
    }
}