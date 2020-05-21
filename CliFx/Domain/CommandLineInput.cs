using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal partial class CommandLineInput
    {
        public IReadOnlyList<string> Directives { get; }

        public string? CommandName { get; }

        public IReadOnlyList<string> Parameters { get; }

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Options { get; }

        public CommandLineInput(
            IReadOnlyList<string> directives,
            string? commandName,
            IReadOnlyList<string> parameters,
            IReadOnlyDictionary<string, IReadOnlyList<string>> options)
        {
            Directives = directives;
            CommandName = commandName;
            Parameters = parameters;
            Options = options;
        }
    }

    internal partial class CommandLineInput
    {
        private static IReadOnlyList<string> ParseDirectives(IReadOnlyList<string> commandLineArguments, ref int index)
        {
            var result = new List<string>();

            for (; index < commandLineArguments.Count; index++)
            {
                var argument = commandLineArguments[index];

                if (argument.StartsWith('[') && argument.EndsWith(']'))
                {
                    var directive = argument.Substring(1, argument.Length - 2);
                    result.Add(directive);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static string? ParseCommandName(IReadOnlyList<string> commandLineArguments, ISet<string> commandNames, ref int index)
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

            if (!string.IsNullOrWhiteSpace(commandName))
                index = lastIndex + 1;

            return commandName;
        }

        private static IReadOnlyList<string> ParseParameters(IReadOnlyList<string> commandLineArguments, ref int index)
        {
            var result = new List<string>();

            for (; index < commandLineArguments.Count; index++)
            {
                var argument = commandLineArguments[index];

                if (!argument.StartsWith('-'))
                {
                    result.Add(argument);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<string>> ParseOptions(
            IReadOnlyList<string> commandLineArguments, ref int index)
        {
            var result = new Dictionary<string, List<string>>();

            var currentOptionValues = new List<string>();

            for (; index < commandLineArguments.Count; index++)
            {
                var argument = commandLineArguments[index];

                // Name
                if (argument.StartsWith("--", StringComparison.Ordinal))
                {
                    var alias = argument.Substring(2);
                    currentOptionValues = result[alias] ??= new List<string>();
                }
                // Short name
                else if (argument.StartsWith('-'))
                {
                    foreach (var alias in argument.Substring(1))
                    {
                        currentOptionValues = result[alias.AsString()] ??= new List<string>();
                    }
                }
                // Value
                else
                {
                    currentOptionValues.Add(argument);
                }
            }

            return (IReadOnlyDictionary<string, IReadOnlyList<string>>) result;
        }

        public static CommandLineInput Parse(IReadOnlyList<string> commandLineArguments, IReadOnlyList<string> availableCommandNames)
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

            return new CommandLineInput(directives, commandName, parameters, options);
        }
    }

    internal partial class CommandLineInput
    {
        public static CommandLineInput Empty { get; } = new CommandLineInput(
            Array.Empty<string>(),
            null,
            Array.Empty<string>(),
            new Dictionary<string, IReadOnlyList<string>>()
        );
    }
}