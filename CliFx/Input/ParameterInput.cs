namespace CliFx.Input;

internal class ParameterInput
{
    public string Value { get; }

    public ParameterInput(string value) => Value = value;

    public string GetFormattedIdentifier() => $"<{Value}>";
}