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

        public IEnumerable<string> GetSuggestions(IReadOnlyList<string> commandLineArguments)
        {
            var states = new List<ISuggestHandler>();

            var cmd = string.Join(" ", commandLineArguments.Where(arg => !CommandDirectiveInput.IsDirective(arg)));


            var data = new SuggestState
            {
                Arguments = CliFx.Utilities.CommandLineUtils.GetCommandLineArgsV(cmd).Skip(1).ToArray()
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
