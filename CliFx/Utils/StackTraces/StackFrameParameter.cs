namespace CliFx.Utils.StackTraces;

internal class StackFrameParameter(string type, string? name)
{
    public string Type { get; } = type;

    public string? Name { get; } = name;
}
