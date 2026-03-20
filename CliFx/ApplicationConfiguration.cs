using System.Collections.Generic;
using CliFx.Binding;

namespace CliFx;

/// <summary>
/// Configuration of an application.
/// </summary>
public class ApplicationConfiguration(
    IReadOnlyList<CommandDescriptor> commandDescriptors,
    string? debugModeEnvironmentVariable,
    string? previewModeEnvironmentVariable
)
{
    /// <summary>
    /// Command descriptors registered in the application.
    /// </summary>
    public IReadOnlyList<CommandDescriptor> CommandDescriptors { get; } = commandDescriptors;

    /// <summary>
    /// Environment variable name that enables the debug mode.
    /// </summary>
    public string? DebugModeEnvironmentVariable { get; } = debugModeEnvironmentVariable;

    /// <summary>
    /// Environment variable name that enables the preview mode.
    /// </summary>
    public string? PreviewModeEnvironmentVariable { get; } = previewModeEnvironmentVariable;
}
