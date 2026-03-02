using System.Collections.Generic;
using CliFx.Schema;

namespace CliFx.Input;

internal class OptionInput(string identifier, IReadOnlyList<string> values)
{
    public string Identifier { get; } = identifier;

    public IReadOnlyList<string> Values { get; } = values;

    public bool IsHelpOption =>
        CommandOptionSchema.ImplicitHelpOption.MatchesIdentifier(Identifier);

    public bool IsVersionOption =>
        CommandOptionSchema.ImplicitVersionOption.MatchesIdentifier(Identifier);

    public string GetFormattedIdentifier() =>
        Identifier switch
        {
            { Length: >= 2 } => "--" + Identifier,
            _ => '-' + Identifier,
        };
}
