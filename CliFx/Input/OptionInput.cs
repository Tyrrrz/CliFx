using System.Collections.Generic;
using System.Linq;
using CliFx.Schema;
using CliFx.Utils.Extensions;

namespace CliFx.Input
{
    internal class OptionInput
    {
        public string Identifier { get; }

        public IReadOnlyList<string> Values { get; }

        public bool IsHelpOption =>
            OptionSchema.HelpOption.MatchesNameOrShortName(Identifier);

        public bool IsVersionOption =>
            OptionSchema.VersionOption.MatchesNameOrShortName(Identifier);

        public OptionInput(string identifier, IReadOnlyList<string> values)
        {
            Identifier = identifier;
            Values = values;
        }

        public string GetFormattedIdentifier() => Identifier switch
        {
            {Length: >= 2} => "--" + Identifier,
            _ => '-' + Identifier
        };

        // TODO: move?
        public string GetFormattedValues() => Values.Select(v => v.Quote()).JoinToString(" ");
    }
}