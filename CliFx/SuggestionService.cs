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
            var args = commandLineArguments.Where(arg => !CommandDirectiveInput.IsDirective(arg)).ToList();

            if (args.Count == 0)
            {
                return _allCommands;
            }

            string command = "";
            int i = 0;

            // handle command suggestions
            for (; i < args.Count; i++)
            {
                command = string.Join(" ", args.Take(i + 1));

                bool isLastArgument = (i == args.Count - 1);

                // not an exact match to an existing command, return best command suggestions
                if (!_allCommands.Contains(command, StringComparer.OrdinalIgnoreCase))
                {
                    return isLastArgument ? _allCommands.Where(c => c.StartsWith(command, StringComparison.OrdinalIgnoreCase)) : NoSuggestions();
                }

                // exact match found, do we need to look at next argument?
                if (isLastArgument)
                {
                    return NoSuggestions();
                }

                // is the next argument a possible sub command candidate?
                var subCommandCandidate = string.Join(" ", args.Take(i + 2));
                if (_allCommands.Any(c => c.StartsWith(subCommandCandidate, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // next argument is likely to be a parameter or option
                i++;
                break;
            }

            // handle parameter suggestions
            var commandSchema = _schema.TryFindCommand(command);
            var parameterSchemas = new Queue<CommandParameterSchema>(commandSchema?.Parameters.OrderBy(p => p.Order));

            CommandParameterSchema? parameterSchema = null;
            if (parameterSchemas.Count != 0)
            {
                parameterSchema = parameterSchemas.Dequeue();
            }

            for (; i < args.Count; i++)
            {
                // don't give suggestions for parameters we don't know anything about. 
                if (parameterSchemas == null)
                {
                    break;
                }

                var parameter = args.ElementAt(i);

                // stop processing parameters if an option is found
                if (parameter.StartsWith("-"))
                {
                    break;
                }

                // skip parameters we can't give suggestions for (enums, mainly)
                var targetType = parameterSchema?.Property?.PropertyType;
                if (targetType?.IsEnum != true)
                {
                    break;
                }

                bool isLastArgument = (i == args.Count - 1);
                if (isLastArgument)
                {
                    return Enum.GetNames(targetType)
                               .Where(p => p.StartsWith(parameter, StringComparison.OrdinalIgnoreCase));
                }

                if (parameterSchema?.IsScalar == true)
                {
                    parameterSchema = null;
                    if (parameterSchemas.Count != 0)
                    {
                        parameterSchema = parameterSchemas.Dequeue();
                    }
                }
            }

            return NoSuggestions();
        }

        private List<string> NoSuggestions()
        {
            return new List<string>();
        }
    }
}
