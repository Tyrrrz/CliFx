using System.Collections.Generic;
using CliFx.Schema;

namespace CliFx.Input
{
    internal class OptionInput
    {
        public string Identifier { get; }

        public IReadOnlyList<string> Values { get; }

        public string RawText { get; }

        public bool IsHelpOption =>
            OptionSchema.HelpOption.MatchesIdentifier(Identifier);

        public bool IsVersionOption =>
            OptionSchema.VersionOption.MatchesIdentifier(Identifier);

        public OptionInput(string identifier, IReadOnlyList<string> values, string rawText)
        {
            Identifier = identifier;
            Values = values;
            RawText = rawText;
        }


        public string GetFormattedIdentifier() => Identifier switch
        {
            {Length: >= 2} => "--" + Identifier,
            _ => '-' + Identifier
        };
    }
}