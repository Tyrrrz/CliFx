using System;

namespace CliFx.Input;

public class DirectiveInput(string name)
{
    public string Name { get; } = name;

    public bool IsDebugDirective =>
        string.Equals(Name, "debug", StringComparison.OrdinalIgnoreCase);

    public bool IsPreviewDirective =>
        string.Equals(Name, "preview", StringComparison.OrdinalIgnoreCase);
}
