using System;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Domain.Suggest
{
    internal class CommandSuggestHandler : AbstractSuggestHandler
    {
        private IEnumerable<string> _allCommands;

        public CommandSuggestHandler(IEnumerable<string> allCommands)
        {
            _allCommands = allCommands;
        }

        public override void Execute(SuggestionData data)
        {
            if (data.Arguments.Count == 0)
            {
                StopProcessing = true;
                data.Suggestions = _allCommands;
            }

            for (; data.Index < data.Arguments.Count; data.Index++)
            {
                data.Command = string.Join(" ", data.Arguments.Take(data.Index + 1));

                bool isLastArgument = data.Index == data.Arguments.Count - 1;

                // not an exact match to an existing command, return best command suggestions
                if (!_allCommands.Contains(data.Command, StringComparer.OrdinalIgnoreCase))
                {
                    StopProcessing = true;
                    data.Suggestions = isLastArgument ? _allCommands.Where(c => c.StartsWith(data.Command, StringComparison.OrdinalIgnoreCase)) : NoSuggestions();
                    return;
                }

                // exact match found, do we need to look at next argument?
                if (isLastArgument)
                {
                    StopProcessing = true;
                    data.Suggestions = NoSuggestions();
                    return;
                }

                // is the next argument a possible sub command candidate?
                var subCommandCandidate = string.Join(" ", data.Arguments.Take(data.Index + 2));
                if (_allCommands.Any(c => c.StartsWith(subCommandCandidate, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // next argument is likely to be a parameter or option
                data.Index++;
                break;
            }
        }
    }
}
