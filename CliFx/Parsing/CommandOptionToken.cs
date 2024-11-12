using System.Collections.Generic;

namespace CliFx.Parsing;

/// <summary>
/// Command-line arguments that provide one or more values to an option input of a command.
/// </summary>
public class CommandOptionToken(string identifier, IReadOnlyList<string> values)
{
    /// <summary>
    /// Option identifier (either name or short name).
    /// </summary>
    public string Identifier { get; } = identifier;

    internal string FormattedIdentifier { get; } =
        identifier switch
        {
            { Length: >= 2 } => "--" + identifier,
            _ => '-' + identifier,
        };

    /// <summary>
    /// Option value(s).
    /// </summary>
    public IReadOnlyList<string> Values { get; } = values;
}
