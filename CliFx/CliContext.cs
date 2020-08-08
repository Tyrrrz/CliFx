namespace CliFx
{
    /// <inheritdoc/>
    public class CliContext : ICliContext
    {
        /// <inheritdoc/>
        public bool IsInteractive { get; internal set; }

        /// <inheritdoc/>
        public ApplicationMetadata Metadata { get; }

        /// <inheritdoc/>
        public ApplicationConfiguration Configuration { get; }

        /// <inheritdoc/>
        public IConsole Console { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CliContext"/>.
        /// </summary>
        public CliContext(ApplicationMetadata metadata, ApplicationConfiguration applicationConfiguration, IConsole console)
        {
            IsInteractive = false;
            Metadata = metadata;
            Configuration = applicationConfiguration;
            Console = console;
        }
    }
}
