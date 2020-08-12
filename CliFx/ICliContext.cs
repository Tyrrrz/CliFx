using CliFx.Domain;
using CliFx.Domain.Input;

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
        /// Console instance.
        /// </summary>
        IConsole Console { get; }

        /// <summary>
        /// Root schema (null value when not resolved).
        /// </summary>
        RootSchema Root { get; }

        /// <summary>
        /// Parsed CLI input.
        /// </summary>
        CommandInput CurrentInput { get; }

        /// <summary>
        /// Current command schema (null value when not in command context).
        /// </summary>
        CommandSchema CurrentCommand { get; }
    }
}
