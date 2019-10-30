using System.Threading;
using System.Threading.Tasks;
using CliFx.Services;

namespace CliFx
{
    /// <summary>
    /// Point of interaction between a user and command line interface.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes command using specified implementation of <see cref="IConsole"/>.
        /// This method is called when the command is invoked by a user through command line interface.
        /// </summary>
        Task ExecuteAsync(IConsole console, CancellationToken cancellationToken);
    }
}