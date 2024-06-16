namespace CliFx.Input;

/// <summary>
/// Input provided by the means of a parameter.
/// </summary>
public class ParameterInput(int order, string value)
{
    /// <summary>
    /// Parameter order.
    /// </summary>
    public int Order { get; } = order;

    /// <summary>
    /// Parameter value.
    /// </summary>
    public string Value { get; } = value;

    internal string GetFormattedIdentifier() => $"<{Value}>";
}
