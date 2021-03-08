using CliFx.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            var data = new SuggestState
            {
                Arguments = commandLineArguments.Where(arg => !CommandDirectiveInput.IsDirective(arg)).ToList(),
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
