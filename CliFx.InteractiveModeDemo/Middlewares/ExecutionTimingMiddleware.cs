namespace CliFx.InteractiveModeDemo.Middlewares
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class ExecutionTimingMiddleware : ICommandMiddleware
    {
        public async Task HandleAsync(ICliContext context, CommandPipelineHandlerDelegate next, CancellationToken cancellationToken)
        {
            context.Console.Output.WriteLine("-- Handling Command");
            Stopwatch stopwatch = Stopwatch.StartNew();

            await next(context, cancellationToken);

            stopwatch.Stop();
            context.Console.Output.WriteLine("-- Finished Command after {0} ms", stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
