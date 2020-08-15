namespace CliFx
{
    using System;
    using System.Collections.Generic;
    using CliFx.Input;
    using CliFx.Schemas;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc/>
    public class CliContext : ICliContext
    {
        private RootSchema? root;
        private CommandInput? currentInput;
        private CommandSchema? currentCommand;
        private ICommand? command;
        private IReadOnlyDictionary<ArgumentSchema, object?>? commandDefaultValues;

        /// <inheritdoc/>
        public bool IsInteractiveMode { get; internal set; }

        /// <inheritdoc/>
        public string Scope { get; set; } = string.Empty;

        /// <inheritdoc/>
        public ApplicationMetadata Metadata { get; }

        /// <inheritdoc/>
        public ApplicationConfiguration Configuration { get; }

        /// <inheritdoc/>
        public IEnumerable<ServiceDescriptor> Services { get; }

        /// <inheritdoc/>
        public IConsole Console { get; }

        /// <inheritdoc/>
        public RootSchema RootSchema
        {
            get => root ?? throw new NullReferenceException("Root schema is not initialized in this context.");
            internal set => root = value;
        }

        /// <inheritdoc/>
        public CommandInput Input
        {
            get => currentInput ?? throw new NullReferenceException("Input is not initialized in this context.");
            internal set => currentInput = value;
        }

        /// <inheritdoc/>
        public CommandSchema CommandSchema
        {
            get => currentCommand ?? throw new NullReferenceException("Current command schema is not initialized in this context.");
            internal set => currentCommand = value;
        }

        /// <inheritdoc/>
        public ICommand Command
        {
            get => command ?? throw new NullReferenceException("Current command is not initialized in this context.");
            internal set => command = value;
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<ArgumentSchema, object?> CommandDefaultValues
        {
            get => commandDefaultValues ?? throw new NullReferenceException("Current command default values is not initialized in this context.");
            internal set => commandDefaultValues = value;
        }

        /// <inheritdoc/>
        public int? ExitCode { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="CliContext"/>.
        /// </summary>
        public CliContext(ApplicationMetadata metadata,
                          ApplicationConfiguration applicationConfiguration,
                          ServiceCollection serviceCollection,
                          IConsole console)
        {
            IsInteractiveMode = false;
            Metadata = metadata;
            Configuration = applicationConfiguration;
            Services = serviceCollection;
            Console = console;
        }
    }
}
