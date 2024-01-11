using System;
using System.Collections.Generic;

namespace CliFx;

/// <summary>
/// Configuration of an application.
/// </summary>
public class ApplicationConfiguration(
    IReadOnlyList<Type> commandTypes,
    bool isDebugModeAllowed,
    bool isPreviewModeAllowed
)
{
    /// <summary>
    /// Command types defined in the application.
    /// </summary>
    public IReadOnlyList<Type> CommandTypes { get; } = commandTypes;

    /// <summary>
    /// Whether debug mode is allowed in the application.
    /// </summary>
    public bool IsDebugModeAllowed { get; } = isDebugModeAllowed;

    /// <summary>
    /// Whether preview mode is allowed in the application.
    /// </summary>
    public bool IsPreviewModeAllowed { get; } = isPreviewModeAllowed;
}
