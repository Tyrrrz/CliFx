using CliFx.Domain;

namespace CliFx
{
    /// <inheritdoc/>
    public class CliContext : ICliContext
    {
        /// <inheritdoc/>
        public bool IsInteractive { get; internal set; }

        /// <inheritdoc/>
        public string Scope { get; internal set; } = string.Empty;

        /// <inheritdoc/>
        public ApplicationMetadata Metadata { get; }

        /// <inheritdoc/>
        public ApplicationConfiguration Configuration { get; }

        /// <inheritdoc/>
        public IConsole Console { get; }

        /// <inheritdoc/>
        public RootSchema? Root { get; internal set; }

        /// <inheritdoc/>
        public CommandSchema? CurrentCommandSchema { get; internal set; }

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
