namespace CliFx
{
    /// <summary>
    /// Command line application context.
    /// </summary>
    public interface ICliContext
    {
        /// <summary>
        /// Whether the application is running in interactive mode.
        /// </summary>
        bool IsInteractive { get; }

        /// <summary>
        /// Current command sope in interactive mode.
        /// </summary>
        string Scope { get; }

        /// <summary>
        /// Metadata associated with an application.
        /// </summary>
        ApplicationMetadata Metadata { get; }

        /// <summary>
        /// Configuration of an application.
        /// </summary>
        ApplicationConfiguration Configuration { get; }

        /// <summary>
        /// Console instance.
        /// </summary>
        IConsole Console { get; }

        /// <summary>
        /// Infomrations about current command.
        /// </summary>
        CommandInfo? CurrentCommandInfo { get; }
    }
}
