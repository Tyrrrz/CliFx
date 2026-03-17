namespace CliFx.Parsing;

internal class ParsedPositionalArgument(string value)
{
    public string Value { get; } = value;

    public override string ToString() => $"<{Value}>";
}
