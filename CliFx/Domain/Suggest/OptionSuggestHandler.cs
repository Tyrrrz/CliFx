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

            // hack: handle edge case where --help and -h are applicable but there is no command available.
            var names = new[] { "help" }
                            .Concat(commandSchema == null ? new string[] { } : commandSchema.Options?.Select(p => p.Name))
                            .Distinct();
            var shortNames = new[] { "h" }
                            .Concat(commandSchema == null ? new string[] { } : commandSchema.Options?.Select(p => p.ShortName?.ToString()))
                            .Distinct();

            // hack: skip to the last argument.             
            data.Index = data.Arguments.Count - 1;
            var optionArg = data.Arguments[data.Index];

            if (optionArg.StartsWith("--"))
            {
                var option = optionArg.Substring(2);
                data.Suggestions = names.Where(n => n?.StartsWith(option) == true).Select(p => $"--{p}");
            }
            else if (optionArg.StartsWith("-"))
            {
                data.Suggestions = shortNames.Select(p => $"-{p}")
                                .Concat(names.Select(p=> $"--{p}") );
            }
            StopProcessing = true;
        }
    }
}
