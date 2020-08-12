using System;
using CliFx.Domain;
using CliFx.Domain.Input;

namespace CliFx
{
    /// <inheritdoc/>
    public class CliContext : ICliContext
    {
        private RootSchema? root;
        private CommandInput? currentInput;
        private CommandSchema? currentCommand;

        /// <inheritdoc/>
        public bool IsInteractiveMode { get; internal set; }

        /// <inheritdoc/>
        public string Scope { get; set; } = string.Empty;

        /// <inheritdoc/>
        public ApplicationMetadata Metadata { get; }

        /// <inheritdoc/>
        public ApplicationConfiguration Configuration { get; }

        /// <inheritdoc/>
        public IConsole Console { get; }

        /// <inheritdoc/>
        public RootSchema Root
        {
            get => root ?? throw new NullReferenceException("Root schema is not initialized in this context.");
            internal set => root = value;
        }

        /// <inheritdoc/>
        public CommandInput CurrentInput
        {
            get => currentInput ?? throw new NullReferenceException("Current input is not initialized in this context.");
            internal set => currentInput = value;
        }

        /// <inheritdoc/>
        public CommandSchema CurrentCommand
        {
            get => currentCommand ?? throw new NullReferenceException("Current command is not initialized in this context.");
            internal set => currentCommand = value;
        }

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
