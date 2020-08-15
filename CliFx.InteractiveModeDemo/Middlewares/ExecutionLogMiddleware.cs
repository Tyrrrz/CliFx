namespace CliFx.InteractiveModeDemo.Middlewares
{
    using System.Threading;
    using System.Threading.Tasks;
    using CliFx.InteractiveModeDemo.Services;

    public sealed class ExecutionLogMiddleware : ICommandMiddleware
    {
        private readonly LibraryService _library;

        public ExecutionLogMiddleware(LibraryService library)
        {
            _library = library;
        }

        public async Task HandleAsync(ICliContext context, CommandPipelineHandlerDelegate next, CancellationToken cancellationToken)
        {
            context.Console.Output.WriteLine($"-- Log Command {_library.GetLibrary().Books.Count}");

            await next(context, cancellationToken);

            context.Console.Output.WriteLine("-- Finished Log Command");
        }
    }
}
