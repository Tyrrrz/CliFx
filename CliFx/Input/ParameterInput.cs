namespace CliFx.Input;

internal class ParameterInput(string value)
{
    public string Value { get; } = value;

    public string GetFormattedIdentifier() => $"<{Value}>";
}
