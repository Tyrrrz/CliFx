using System.Collections.Generic;
using System.Linq;
using CliFx.Internal.Extensions;

namespace CliFx.Domain
{
    internal class CommandOptionInput
    {
        public string Alias { get; }

        public IReadOnlyList<string> Values { get; }

        public bool IsHelpOption => CommandOptionSchema.HelpOption.MatchesNameOrShortName(Alias);

        public bool IsVersionOption => CommandOptionSchema.VersionOption.MatchesNameOrShortName(Alias);

        public CommandOptionInput(string alias, IReadOnlyList<string> values)
        {
            Alias = alias;
            Values = values;
        }

        public string GetRawAlias() => Alias switch
        {
            { Length: 0 } => Alias,
            { Length: 1 } => $"-{Alias}",
            _ => $"--{Alias}"
        };

        public string GetRawValues() => Values.Select(v => v.Quote()).JoinToString(" ");

        public override string ToString() => $"{GetRawAlias()} {GetRawValues()}";
    }
}