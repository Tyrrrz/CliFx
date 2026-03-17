namespace CliFx.Parsing;

internal class ParsedPositionalArgument(string value)
{
    public string Value { get; } = value;

    public string GetFormattedIdentifier() => $"<{Value}>";
}
