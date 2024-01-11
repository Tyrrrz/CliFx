namespace CliFx;

/// <summary>
/// Metadata associated with an application.
/// </summary>
public class ApplicationMetadata(
    string title,
    string executableName,
    string version,
    string? description
)
{
    /// <summary>
    /// Application title.
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// Application executable name.
    /// </summary>
    public string ExecutableName { get; } = executableName;

    /// <summary>
    /// Application version.
    /// </summary>
    public string Version { get; } = version;

    /// <summary>
    /// Application description.
    /// </summary>
    public string? Description { get; } = description;
}
