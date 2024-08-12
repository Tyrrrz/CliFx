using System.Collections.Generic;

namespace CliFx.Input;

/// <summary>
/// Input provided by the means of an option.
/// </summary>
public class CommandOptionInput(string identifier, IReadOnlyList<string> values)
{
    /// <summary>
    /// Option identifier (either the name or the short name).
    /// </summary>
    public string Identifier { get; } = identifier;

    internal string FormattedIdentifier { get; } =
        identifier switch
        {
            { Length: >= 2 } => "--" + identifier,
            _ => '-' + identifier
        };

    /// <summary>
    /// Option value(s).
    /// </summary>
    public IReadOnlyList<string> Values { get; } = values;
}
