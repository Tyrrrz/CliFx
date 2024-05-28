using CliFx.Schema;

namespace CliFx;

/// <summary>
/// Configuration of an application.
/// </summary>
public class ApplicationConfiguration(
    ApplicationSchema schema,
    bool isDebugModeAllowed,
    bool isPreviewModeAllowed
)
{
    /// <summary>
    /// Application schema.
    /// </summary>
    public ApplicationSchema Schema { get; } = schema;

    /// <summary>
    /// Whether debug mode is allowed in the application.
    /// </summary>
    public bool IsDebugModeAllowed { get; } = isDebugModeAllowed;

    /// <summary>
    /// Whether preview mode is allowed in the application.
    /// </summary>
    public bool IsPreviewModeAllowed { get; } = isPreviewModeAllowed;
}
