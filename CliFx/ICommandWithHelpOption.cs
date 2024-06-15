namespace CliFx;

/// <summary>
/// Command definition that includes the help option.
/// </summary>
public interface ICommandWithHelpOption : ICommand
{
    /// <summary>
    /// Whether the user requested help for this command (via the `-h|--help` option).
    /// </summary>
    bool IsHelpRequested { get; }
}
