using System.Collections.Generic;

namespace CliFx.Input;

internal class OptionInput(string identifier, IReadOnlyList<string> values)
{
    public string Identifier { get; } = identifier;

    public IReadOnlyList<string> Values { get; } = values;

    public bool IsHelpOption =>
        string.Equals(Identifier, "help", System.StringComparison.OrdinalIgnoreCase)
        || (Identifier.Length == 1 && Identifier[0] == 'h');

    public bool IsVersionOption =>
        string.Equals(Identifier, "version", System.StringComparison.OrdinalIgnoreCase);

    public string GetFormattedIdentifier() =>
        Identifier switch
        {
            { Length: >= 2 } => "--" + Identifier,
            _ => '-' + Identifier,
        };
}
