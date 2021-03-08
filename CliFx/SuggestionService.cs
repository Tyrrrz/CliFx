using CliFx.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CliFx
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
            var nonDirectiveArgs = commandLineArguments.Where(arg => !CommandDirectiveInput.IsDirective(arg)).ToList();

            if( nonDirectiveArgs.Count == 0)
            {
                return _allCommands;
            }

            var command = nonDirectiveArgs[0];

            if (_allCommands.Contains(command))
            {
                return NoSuggestions();
            }

            return _allCommands.Where(c => c.StartsWith(command, StringComparison.OrdinalIgnoreCase));
        }

        private List<string> NoSuggestions()
        {
            return new List<string>();
        }
    }
}
