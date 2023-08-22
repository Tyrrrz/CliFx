using System;

namespace CliFx.Input;

internal class DirectiveInput
{
    public string Name { get; }

    public bool IsDebugDirective =>
        string.Equals(Name, "debug", StringComparison.OrdinalIgnoreCase);

    public bool IsPreviewDirective =>
        string.Equals(Name, "preview", StringComparison.OrdinalIgnoreCase);

    public DirectiveInput(string name) => Name = name;
}
