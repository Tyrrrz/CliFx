using System.Threading;
using System.Threading.Tasks;

namespace CliFx
{
    /// <summary>
    /// Represents an async continuation for the next task to execute in the pipeline.
    /// </summary>
    /// <returns>Awaitable task</returns>
    public delegate Task CommandPipelineHandlerDelegate(ICliContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Pipeline middleware to surround the inner handler.
    /// Implementations add additional behavior and await the next delegate.
    /// </summary>
    public interface ICommandMiddleware
    {
        /// <summary>
        /// Pipeline handler. Perform any additional behavior and await the next delegate as necessary
        /// </summary>
        /// <returns></returns>
        Task HandleAsync(ICliContext context, CommandPipelineHandlerDelegate next, CancellationToken cancellationToken);
    }
}
