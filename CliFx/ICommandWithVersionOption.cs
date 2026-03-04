namespace CliFx;

/// <summary>
/// Command that has the conventional version option defined.
/// </summary>
/// <remarks>
/// This interface is implemented automatically by a source generator and shouldn't be used directly.
/// </remarks>
public interface ICommandWithVersionOption : ICommand
{
    /// <summary>
    /// Whether the user requested the application version information (via the <c>--version</c> option).
    /// </summary>
    /// <remarks>
    /// If this property is <c>true</c>, the version text is automatically rendered by the framework
    /// and the host command's <see cref="ICommand.ExecuteAsync"/> method does not get called.
    /// </remarks>
    bool IsVersionRequested { get; }
}
