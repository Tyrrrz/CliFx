using System.Collections.Generic;
using System.Text;

namespace CliFx.Parsing;

internal class ParsedOption(string identifier, IReadOnlyList<string> values)
{
    public string Identifier { get; } = identifier;

    public IReadOnlyList<string> Values { get; } = values;

    public override string ToString()
    {
        var buffer = new StringBuilder();

        // Identifier prefixed by dash(es)
        buffer.Append(
            Identifier switch
            {
                { Length: >= 2 } => "--" + Identifier,
                _ => '-' + Identifier,
            }
        );

        // Value(s) wrapped in quotes
        foreach (var value in Values)
        {
            if (string.IsNullOrEmpty(value))
                continue;

            buffer.Append(' ');
            buffer.Append('<');
            buffer.Append(value);
            buffer.Append('>');
        }

        return buffer.ToString();
    }
}
