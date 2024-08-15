using System;

namespace CliFx.Parsing;

/// <summary>
/// Command-line argument that sets a directive.
/// </summary>
public class CommandDirectiveToken(string name)
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
