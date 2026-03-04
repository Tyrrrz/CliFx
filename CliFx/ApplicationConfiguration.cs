using System.Collections.Generic;
using CliFx.Schema;

namespace CliFx;

/// <summary>
/// Configuration of an application.
/// </summary>
public class ApplicationConfiguration(
    IReadOnlyList<CommandSchema> commandSchemas,
    bool isDebugModeAllowed,
    bool isPreviewModeAllowed
)
{
    /// <summary>
    /// Command schemas registered in the application.
    /// </summary>
    public IReadOnlyList<CommandSchema> CommandSchemas { get; } = commandSchemas;

    /// <summary>
    /// Whether debug mode is allowed in the application.
    /// </summary>
    public bool IsDebugModeAllowed { get; } = isDebugModeAllowed;

    /// <summary>
    /// Whether preview mode is allowed in the application.
    /// </summary>
    public bool IsPreviewModeAllowed { get; } = isPreviewModeAllowed;
}
