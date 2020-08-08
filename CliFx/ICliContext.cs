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
        public bool IsInteractive { get; }

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
    }
}
