namespace CliFx.InteractiveModeDemo.Middlewares
{
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class ExecutionTimingMiddleware : ICommandMiddleware
    {
        public async Task HandleAsync(ICliContext context, CommandPipelineHandlerDelegate next, CancellationToken cancellationToken)
        {
            context.Console.Output.WriteLine("-- Handling Command");

            await next(context, cancellationToken);

            context.Console.Output.WriteLine("-- Finished Command");
        }
    }
}
