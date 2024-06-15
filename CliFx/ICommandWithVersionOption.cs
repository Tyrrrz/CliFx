namespace CliFx;

/// <summary>
/// Command definition that includes the version option.
/// </summary>
public interface ICommandWithVersionOption : ICommand
{
    /// <summary>
    /// Whether the user requested the application version information (via the `--version` option).
    /// </summary>
    bool IsVersionRequested { get; }
}
