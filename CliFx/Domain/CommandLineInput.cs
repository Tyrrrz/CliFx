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
        private static IReadOnlyList<string> ParseDirectives(IEnumerable<string> commandLineArguments, out int lastIndex)
        {
            var result = new List<string>();
            lastIndex = 0;

            foreach (var argument in commandLineArguments)
            {
                if (argument.StartsWith('[') && argument.EndsWith(']'))
                {
                    var directive = argument.Substring(1, argument.Length - 2);
                    result.Add(directive);
                }
                else
                {
                    break;
                }

                lastIndex++;
            }

            return result;
        }

        private static string? ParseCommandName(IEnumerable<string> commandLineArguments, ISet<string> commandNames, out int lastIndex)
        {
            var commandName = default(string?);
            lastIndex = 0;

            var buffer = new List<string>();

            var i = 0;
            foreach (var argument in commandLineArguments)
            {
                buffer.Add(argument);

                var potentialCommandName = buffer.JoinToString(" ");

                if (commandNames.Contains(potentialCommandName))
                {
                    commandName = potentialCommandName;
                    lastIndex = i;
                }

                i++;
            }

            return commandName;
        }

        private static IReadOnlyList<string> ParseParameters(IEnumerable<string> commandLineArguments, out int lastIndex)
        {
            var result = new List<string>();
            lastIndex = 0;

            foreach (var argument in commandLineArguments)
            {
                if (!argument.StartsWith('-'))
                {
                    result.Add(argument);
                }
                else
                {
                    break;
                }

                lastIndex++;
            }

            return result;
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<string>> ParseOptions(IEnumerable<string> commandLineArguments)
        {
            var result = new Dictionary<string, IReadOnlyList<string>>();

            var currentOptionAlias = default(string?);
            var currentOptionValues = new List<string>();

            foreach (var argument in commandLineArguments)
            {
                // Short name
                if (argument.StartsWith("--", StringComparison.Ordinal))
                {
                    // Flush
                    if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                        result[currentOptionAlias] = currentOptionValues;

                    currentOptionAlias = argument.Substring(2);
                    currentOptionValues = new List<string>();
                }
                else if (argument.StartsWith('-'))
                {
                    // Flush
                    if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                        result[currentOptionAlias] = currentOptionValues;

                    foreach (var alias in argument.Substring(1))
                    {
                        currentOptionAlias = alias.AsString();
                        currentOptionValues = new List<string>();
                    }
                }
                else
                {
                    currentOptionValues.Add(argument);
                }
            }

            return result;
        }

        public static CommandLineInput Parse(IReadOnlyList<string> commandLineArguments, IReadOnlyList<string> availableCommandNames)
        {
            var availableCommandNamesSet = availableCommandNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var directives = ParseDirectives(
                commandLineArguments,
                out var directivesLastIndex
            );

            var commandName = ParseCommandName(
                commandLineArguments.Skip(directivesLastIndex),
                availableCommandNamesSet,
                out var commandNameLastIndex
            );

            var parameters = ParseParameters(
                commandLineArguments.Skip(commandNameLastIndex),
                out var parametersLastIndex
            );

            var options = ParseOptions(
                commandLineArguments.Skip(parametersLastIndex)
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