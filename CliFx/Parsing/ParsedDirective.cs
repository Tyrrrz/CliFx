using System;

namespace CliFx.Parsing;

internal class ParsedDirective(string name)
{
    public string Name { get; } = name;

    public bool IsDebugDirective =>
        string.Equals(Name, "debug", StringComparison.OrdinalIgnoreCase);

    public bool IsPreviewDirective =>
        string.Equals(Name, "preview", StringComparison.OrdinalIgnoreCase);

    public override string ToString() => '[' + Name + ']';
}
