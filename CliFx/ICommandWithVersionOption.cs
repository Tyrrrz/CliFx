namespace CliFx;

/// <summary>
/// Command that has the conventional version option defined.
/// </summary>
/// <remarks>
/// This interface is implemented automatically by the source generator and should not be used directly.
/// </remarks>
// Note: this interface intentionally does not inherit from ICommand.
// This is done so that, when the source generator implements this interface on a command class,
// the user is not suggested to remove their own `: ICommand` declaration due to redundancy.
public interface ICommandWithVersionOption
{
    /// <summary>
    /// Whether the user requested the application version information.
    /// </summary>
    /// <remarks>
    /// If this property is <c>true</c>, the version text is rendered automatically by the framework,
    /// skipping the command's <see cref="ICommand.ExecuteAsync" /> method entirely.
    /// </remarks>
    bool IsVersionRequested { get; }
}
