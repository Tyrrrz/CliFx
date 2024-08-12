using System;

namespace CliFx.Input;

/// <summary>
/// Input provided by the means of a directive.
/// </summary>
public class CommandDirectiveInput(string name)
{
    /// <summary>
    /// Directive name.
    /// </summary>
    public string Name { get; } = name;

    internal bool IsDebugDirective =>
        string.Equals(Name, "debug", StringComparison.OrdinalIgnoreCase);

    internal bool IsPreviewDirective =>
        string.Equals(Name, "preview", StringComparison.OrdinalIgnoreCase);
}
