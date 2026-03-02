using System;
using System.Collections.Generic;
using CliFx.Schema;

namespace CliFx;

/// <summary>
/// Configuration of an application.
/// </summary>
public class ApplicationConfiguration(
    IReadOnlyList<Type> commandTypes,
    bool isDebugModeAllowed,
    bool isPreviewModeAllowed,
    IReadOnlyList<CommandSchema>? commandSchemas = null
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

    /// <summary>
    /// Pre-built command schemas (e.g., from the source generator).
    /// When non-null and non-empty, these are used directly by <see cref="CliApplication" />
    /// instead of resolving schemas from <see cref="CommandTypes" /> via reflection.
    /// </summary>
    public IReadOnlyList<CommandSchema>? CommandSchemas { get; } = commandSchemas;
}
