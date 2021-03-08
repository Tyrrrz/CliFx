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

            string command = "";

            for (int i = 0; i < nonDirectiveArgs.Count; i++)
            {
                command = string.Join(" ", nonDirectiveArgs.Take(i + 1));

                if (_allCommands.Contains(command, StringComparer.OrdinalIgnoreCase))
                {
                    return NoSuggestions();
                }                
            }

            return _allCommands.Where(c => c.StartsWith(command, StringComparison.OrdinalIgnoreCase));
        }

        private List<string> NoSuggestions()
        {
            return new List<string>();
        }
    }
}
