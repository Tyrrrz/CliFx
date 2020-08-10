using System;
using CliFx.Domain;
using CliFx.Domain.Input;

namespace CliFx
{
    /// <inheritdoc/>
    public class CliContext : ICliContext
    {
        /// <inheritdoc/>
        public bool IsInteractiveMode { get; internal set; }

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

        //TODO: change nullable to full property and thorw excpetion if null

        /// <inheritdoc/>
        public CommandSchema? CurrentCommand { get; internal set; }

        /// <inheritdoc/>
        public CommandInput? CurrentInput { get; internal set; }

        /// <summary>
        /// Initializes an instance of <see cref="CliContext"/>.
        /// </summary>
        public CliContext(ApplicationMetadata metadata, ApplicationConfiguration applicationConfiguration, IConsole console)
        {
            IsInteractiveMode = false;
            Metadata = metadata;
            Configuration = applicationConfiguration;
            Console = console;
        }
    }
}
