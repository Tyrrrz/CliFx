namespace CliFx.Input;

/// <summary>
/// Describes the materialized input for a parameter of a command.
/// </summary>
public class ParameterInput(string value)
{
    /// <summary>
    /// Parameter value.
    /// </summary>
    public string Value { get; } = value;

    internal string GetFormattedIdentifier() => $"<{Value}>";
}
