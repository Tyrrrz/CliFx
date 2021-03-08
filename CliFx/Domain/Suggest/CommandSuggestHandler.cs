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

        public override void Execute(SuggestState state)
        {
            if (state.Arguments.Count == 0)
            {
                StopProcessing = true;
                state.Suggestions = _allCommands;
            }

            for (; state.Index < state.Arguments.Count; state.Index++)
            {
                state.CommandCandidate = string.Join(" ", state.Arguments.Take(state.Index + 1));

                // not an exact match to an existing command, return best command suggestions
                if (!_allCommands.Contains(state.CommandCandidate, StringComparer.OrdinalIgnoreCase))
                {
                    StopProcessing = true;
                    state.Suggestions = state.IsLastArgument() 
                                            ? _allCommands.Where(c => c.StartsWith(state.CommandCandidate, StringComparison.OrdinalIgnoreCase))
                                            : NoSuggestions();
                    return;
                }

                // exact match found, do we need to look at next argument?
                if (state.IsLastArgument())
                {
                    StopProcessing = true;
                    state.Suggestions = NoSuggestions();
                    return;
                }

                // is the next argument a possible sub command candidate?
                var subCommandCandidate = string.Join(" ", state.Arguments.Take(state.Index + 2));
                if (_allCommands.Any(c => c.StartsWith(subCommandCandidate, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // next argument is likely to be a parameter or option
                state.Index++;
                break;
            }
        }
    }
}
