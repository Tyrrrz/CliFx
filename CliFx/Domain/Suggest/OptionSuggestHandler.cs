using System;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Domain.Suggest
{
    internal class OptionSuggestHandler : AbstractSuggestHandler
    {
        private RootSchema _schema;

        public OptionSuggestHandler(RootSchema schema)
        {
            _schema = schema;
        }

        public override void Execute(SuggestState data)
        {
            var commandSchema = _schema.TryFindCommand(data.CommandCandidate);
            var optionSchemas = new List<CommandOptionSchema>(commandSchema?.Options);

            if( data.Index <= data.Arguments.Count - 1)
            {
                data.Index = data.Arguments.Count - 1;
            }

            for (; data.Index < data.Arguments.Count; data.Index++)
            {
                var optionArg = data.Arguments.ElementAt(data.Index);

                if (optionArg.StartsWith("--"))
                {
                    var option = optionArg.Substring(2);
                    data.Suggestions = optionSchemas.Where(o => o?.Name?.StartsWith(option) == true).Select(p => $"--{p.Name}");
                    StopProcessing = true;
                    break;
                }

                if (optionArg.StartsWith("-"))
                {
                    data.Suggestions = optionSchemas.Select(p => $"-{p.ShortName}")
                                    .Concat(optionSchemas.Select(p=> $"--{p.Name}") );
                    StopProcessing = true;
                    break;
                }
            }
        }
    }
}
