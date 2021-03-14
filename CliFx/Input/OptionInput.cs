using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            {Length: 0} => Identifier,
            {Length: 1} => $"-{Identifier}",
            _ => $"--{Identifier}"
        };

        public string GetFormattedValues() => Values.Select(v => v.Quote()).JoinToString(" ");

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"{GetFormattedIdentifier()} {GetFormattedValues()}";
    }
}