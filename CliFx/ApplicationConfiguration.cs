using System.Collections.Generic;
using CliFx.Binding;

namespace CliFx;

/// <summary>
/// Configuration of an application.
/// </summary>
public class ApplicationConfiguration(
    IReadOnlyList<CommandDescriptor> commandDescriptors,
    bool isDebugModeAllowed,
    bool isPreviewModeAllowed
)
{
    /// <summary>
    /// Command descriptors registered in the application.
    /// </summary>
    public IReadOnlyList<CommandDescriptor> CommandDescriptors { get; } = commandDescriptors;

    /// <summary>
    /// Whether debug mode is allowed in the application.
    /// </summary>
    public bool IsDebugModeAllowed { get; } = isDebugModeAllowed;

    /// <summary>
    /// Whether preview mode is allowed in the application.
    /// </summary>
    public bool IsPreviewModeAllowed { get; } = isPreviewModeAllowed;
}
