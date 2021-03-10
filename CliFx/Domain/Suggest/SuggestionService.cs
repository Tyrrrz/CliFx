using CliFx.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CliFx.Domain.Suggest
{
    internal class SuggestionService
    {
        private readonly RootSchema _schema;
        private readonly IReadOnlyList<string> _allCommands;

        public SuggestionService(RootSchema schema)
        {
            _schema = schema;
            _allCommands = _schema.GetCommandNames();
        }

        private string GetCommand(CommandInput input, IReadOnlyList<string> commandLineArguments, IReadOnlyDictionary<string,string> environmentVariables)
        {
            // Accept command line arguments via environment variable as a workaround to powershell escape sequence shennidgans
            var commandCacheVariable = input.Options.FirstOrDefault(p => p.Alias == "envvar")?.Values[0];

            if (commandCacheVariable == null)
            {
                // ignore cursor position as we don't know what the original user input string is
                return string.Join(" ", commandLineArguments.Where(arg => !CommandDirectiveInput.IsDirective(arg)));
            }

            var command = environmentVariables[commandCacheVariable];
            var cursorPositionText = input.Options.FirstOrDefault(p => p.Alias == "cursor")?.Values[0];
            var cursorPosition = command.Length;

            if (int.TryParse(cursorPositionText, out cursorPosition) && cursorPosition < command.Length)
            {
                return command.Remove(cursorPosition);
            }
            return command;
        }

        public IEnumerable<string> GetSuggestions(CommandInput input, IReadOnlyList<string> commandLineArguments, IReadOnlyDictionary<string, string> environmentVariables)
        {
            string command = GetCommand(input, commandLineArguments, environmentVariables);

            var data = new SuggestState
            {
                Arguments = CliFx.Utilities.CommandLineUtils.GetCommandLineArgsV(command).Skip(1).ToArray()
            };

            foreach (var state in new ISuggestHandler[] {
                                new CommandSuggestHandler(_allCommands),
                                new ParameterSuggestHandler(_schema),
                                new OptionSuggestHandler(_schema)})
            {
                state.Execute(data);
                if (state.StopProcessing)
                {
                    break;
                }
            }
            return data.Suggestions;
        }
    }
}
