namespace CliFx
{
    using System.Collections.Generic;
    using CliFx.Input;
    using CliFx.Schemas;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Command line application context.
    /// </summary>
    public interface ICliContext
    {
        /// <summary>
        /// Whether the application is running in interactive mode.
        /// </summary>
        bool IsInteractiveMode { get; }

        /// <summary>
        /// Current command scope in interactive mode.
        /// </summary>
        string Scope { get; set; }

        /// <summary>
        /// Metadata associated with an application.
        /// </summary>
        ApplicationMetadata Metadata { get; }

        /// <summary>
        /// Configuration of an application.
        /// </summary>
        ApplicationConfiguration Configuration { get; }

        /// <summary>
        /// Service collection.
        /// </summary>
        public IEnumerable<ServiceDescriptor> Services { get; }

        /// <summary>
        /// Console instance.
        /// </summary>
        IConsole Console { get; }

        /// <summary>
        /// Root schema.
        /// </summary>
        RootSchema RootSchema { get; }

        /// <summary>
        /// Parsed CLI input.
        /// </summary>
        CommandInput Input { get; }

        /// <summary>
        /// Current command schema.
        /// </summary>
        CommandSchema CommandSchema { get; }

        /// <summary>
        /// Current command instance.
        /// </summary>
        ICommand Command { get; }

        /// <summary>
        /// Collection of command's default values.
        /// </summary>
        IReadOnlyDictionary<ArgumentSchema, object?> CommandDefaultValues { get; }

        /// <summary>
        /// Exit code from current command.
        /// Null if not set. If pipeline exits with null exit code it will be replaced with error exit code (1).
        /// </summary>
        int? ExitCode { get; set; }
    }
}
