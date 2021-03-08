using CliFx.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CliFx.Domain.Suggest
{
    internal class ParameterSuggestHandler : AbstractSuggestHandler
    {
        private RootSchema _schema;

        public ParameterSuggestHandler(RootSchema schema)
        {
            _schema = schema;
        }

        public override void Execute(SuggestionData data)
        {
            // handle parameter suggestions
            var commandSchema = _schema.TryFindCommand(data.Command);
            var parameterSchemas = new Queue<CommandParameterSchema>(commandSchema?.Parameters.OrderBy(p => p.Order));

            CommandParameterSchema? parameterSchema = null;
            if (parameterSchemas.Count != 0)
            {
                parameterSchema = parameterSchemas.Dequeue();
            }

            for (; data.Index < data.Arguments.Count; data.Index++)
            {
                // don't give suggestions for parameters we don't know anything about. 
                if (parameterSchemas == null)
                {
                    break;
                }

                var parameter = data.Arguments.ElementAt(data.Index);

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

                bool isLastArgument = data.Index == data.Arguments.Count - 1;
                if (isLastArgument)
                {
                    StopProcessing = true;
                    data.Suggestions = Enum.GetNames(targetType)
                                            .Where(p => p.StartsWith(parameter, StringComparison.OrdinalIgnoreCase));
                    return;
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
        }
    }
}
