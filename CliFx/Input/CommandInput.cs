using System;
using System.Collections.Generic;
using System.Linq;
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
        // TODO: cleanup below

        private static IReadOnlyList<DirectiveInput> ParseDirectives(
            IReadOnlyList<string> commandLineArguments,
            ref int index)
        {
            var result = new List<DirectiveInput>();

            for (; index < commandLineArguments.Count; index++)
            {
                var argument = commandLineArguments[index];

                if (!argument.StartsWith('[') || !argument.EndsWith(']'))
                    break;

                var name = argument.Substring(1, argument.Length - 2);
                result.Add(new DirectiveInput(name));
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

        private static IReadOnlyList<ParameterInput> ParseParameters(
            IReadOnlyList<string> commandLineArguments,
            ref int index)
        {
            var result = new List<ParameterInput>();

            for (; index < commandLineArguments.Count; index++)
            {
                var argument = commandLineArguments[index];

                var isOptionName =
                    argument.StartsWith("--", StringComparison.OrdinalIgnoreCase) &&
                    argument.Length > 2 &&
                    char.IsLetter(argument[2]);

                var isOptionShortName =
                    argument.StartsWith('-') &&
                    argument.Length > 1 &&
                    char.IsLetter(argument[1]);

                // Break on the first encountered option
                if (isOptionName || isOptionShortName)
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

            var currentOptionAlias = default(string?);
            var currentOptionValues = new List<string>();

            for (; index < commandLineArguments.Count; index++)
            {
                var argument = commandLineArguments[index];

                // Name
                if (argument.StartsWith("--", StringComparison.Ordinal) &&
                    argument.Length > 2 &&
                    char.IsLetter(argument[2]))
                {
                    // Flush previous
                    if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                        result.Add(new OptionInput(currentOptionAlias, currentOptionValues));

                    currentOptionAlias = argument.Substring(2);
                    currentOptionValues = new List<string>();
                }
                // Short name
                else if (argument.StartsWith('-') &&
                         argument.Length > 1 &&
                         char.IsLetter(argument[1]))
                {
                    foreach (var alias in argument.Substring(1))
                    {
                        // Flush previous
                        if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                            result.Add(new OptionInput(currentOptionAlias, currentOptionValues));

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
                result.Add(new OptionInput(currentOptionAlias, currentOptionValues));

            return result;
        }

        public static CommandInput Parse(
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables,
            IReadOnlyList<string> availableCommandNames)
        {
            var availableCommandNamesSet = availableCommandNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var index = 0;

            var parsedDirectives = ParseDirectives(
                commandLineArguments,
                ref index
            );

            var parsedCommandName = ParseCommandName(
                commandLineArguments,
                availableCommandNamesSet,
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