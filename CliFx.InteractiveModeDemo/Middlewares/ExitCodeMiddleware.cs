namespace CliFx.InteractiveModeDemo.Middlewares
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class ExitCodeMiddleware : ICommandMiddleware
    {
        public async Task HandleAsync(ICliContext context, CommandPipelineHandlerDelegate next, CancellationToken cancellationToken)
        {
            await next(context, cancellationToken);

            bool isInteractive = context.IsInteractiveMode;
            int? exitCode = context.ExitCode;

            if (context.ExitCode == 0)
            {
                context.Console.WithForegroundColor(ConsoleColor.White, () =>
                    context.Console.Output.WriteLine($"{context.Metadata.ExecutableName}: Command finished succesfully."));
            }
            else
            {
                context.Console.WithForegroundColor(ConsoleColor.White, () =>
                    context.Console.Output.WriteLine($"{context.Metadata.ExecutableName}: Command finished with exit code ({exitCode})."));
            }
        }
    }
}
