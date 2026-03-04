namespace CliFx;

/// <summary>
/// Command that has the conventional help option defined.
/// </summary>
/// <remarks>
/// This interface is implemented automatically by a source generator and shouldn't be used directly.
/// </remarks>
public interface ICommandWithHelpOption : ICommand
{
    /// <summary>
    /// Whether the user requested help for this command.
    /// </summary>
    /// <remarks>
    /// If this property is <c>true</c>, the help text is automatically rendered by the framework
    /// and the host command's <see cref="ICommand.ExecuteAsync"/> method does not get called.
    /// </remarks>
    bool IsHelpRequested { get; }
}
