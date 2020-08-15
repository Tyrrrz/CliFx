namespace CliFx.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Stores middleware components data.
    /// </summary>
    internal class MiddlewareComponentNode
    {
        private CommandPipelineHandlerDelegate _next;

        /// <summary>
        /// Middleware type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Middleware instance.
        /// </summary>
        public ICommandMiddleware Instance { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CliApplication"/>.
        /// </summary>
        public MiddlewareComponentNode(IServiceProvider serviceProvider,
                                       Type type,
                                       CommandPipelineHandlerDelegate next)
        {
            Type = type;
            Instance = (ICommandMiddleware)serviceProvider.GetRequiredService(Type);
            _next = next;
        }

        /// <summary>
        /// Process middleware pipeline.
        /// </summary>
        public async Task ProcessAsync(ICliContext context, CancellationToken cancellationToken)
        {
            await Instance.HandleAsync(context, _next, cancellationToken);
        }
    }
}