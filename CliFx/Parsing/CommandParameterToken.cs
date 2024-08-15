namespace CliFx.Parsing;

/// <summary>
/// Command-line argument that provide a value to a parameter input of a command.
/// </summary>
public class CommandParameterToken(int order, string value)
{
    /// <summary>
    /// Parameter order.
    /// </summary>
    public int Order { get; } = order;

    /// <summary>
    /// Parameter value.
    /// </summary>
    public string Value { get; } = value;

    internal string FormattedIdentifier { get; } = $"<{value}>";
}
