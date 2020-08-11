using System.Threading.Tasks;

namespace CliFx
{
    /// <summary>
    /// Directive handler.
    /// </summary>
    public interface IDirective
    {
        /// <summary>
        /// Whether to continue execution after exiting the handler.
        /// </summary>
        bool ContinueExecution { get; }

        /// <summary>
        /// Executes the handler using the specified implementation of <see cref="IConsole"/>.
        /// This is the method that's called when a directive is specified by a user through command line.
        /// </summary>
        /// <remarks>
        /// If the execution of the handler is not asynchronous, simply end the method with <code>return default;</code>.
        /// If you want to stop the execution of the command, simply throw DirectiveException.
        /// </remarks>
        ValueTask HandleAsync(IConsole console);
    }
}