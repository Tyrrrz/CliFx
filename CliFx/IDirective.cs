using System.Threading.Tasks;

namespace CliFx
{
    /// <summary>
    /// Directive handler.
    /// </summary>
    public interface IDirective
    {
        /// <summary>
        /// Executes the handler using the specified implementation of <see cref="IConsole"/>.
        /// This is the method that's called when a directive is specified by a user through command line.
        /// Returning not null value will stop the execution of the command with exit specified exit code, while null value can be treated as 'do nothing'.
        /// </summary>
        /// <remarks>If the execution of the handler is not asynchronous, simply end the method with <code>return default;</code></remarks>
        ValueTask<int?> HandleAsync(IConsole console);
    }
}