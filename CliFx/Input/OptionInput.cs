using System.Collections.Generic;

namespace CliFx.Input;

/// <summary>
/// Describes the materialized input for an option of a command.
/// </summary>
public class OptionInput(string identifier, IReadOnlyList<string> values)
{
    /// <summary>
    /// Option identifier (either the name or the short name).
    /// </summary>
    public string Identifier { get; } = identifier;

    /// <summary>
    /// Provided option values.
    /// </summary>
    public IReadOnlyList<string> Values { get; } = values;

    internal string GetFormattedIdentifier() =>
        Identifier switch
        {
            { Length: >= 2 } => "--" + Identifier,
            _ => '-' + Identifier
        };
}
