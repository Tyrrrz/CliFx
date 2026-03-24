namespace CliFx;

/// <summary>
/// Metadata associated with a command-line application.
/// </summary>
public class CommandLineApplicationMetadata(
    string title,
    string executableName,
    string version,
    string? description
)
{
    /// <summary>
    /// Application title.
    /// Used for display purposes in the help text.
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// Application executable name.
    /// Used for display purposes in the help text.
    /// </summary>
    public string ExecutableName { get; } = executableName;

    /// <summary>
    /// Application version.
    /// Used for display purposes in the help text and when the version information is requested.
    /// </summary>
    public string Version { get; } = version;

    /// <summary>
    /// Application description.
    /// Used for display purposes in the help text.
    /// </summary>
    public string? Description { get; } = description;
}
