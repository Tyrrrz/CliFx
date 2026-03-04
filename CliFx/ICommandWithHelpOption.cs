namespace CliFx;

/// <summary>
/// Command definition that includes the help option.
/// </summary>
public interface ICommandWithHelpOption : ICommand
{
    /// <summary>
    /// Whether the user requested help for this command (via the <c>-h|--help</c> option).
    /// </summary>
    bool IsHelpRequested { get; }
}
