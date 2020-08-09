using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.BlazorDemo.CLI.Services;
using CliFx.Exceptions;

namespace CliFx.BlazorDemo.CLI.Commands
{
    [Command("webhost", Description = "Management of the background webhost in the interactive mode.")]
    public class WebHostCommand : ICommand
    {
        private readonly ICliContext _cliContext;
        private readonly IWebHostRunnerService _webHostRunnerService;

        public WebHostCommand(ICliContext cliContext, IWebHostRunnerService webHostRunnerService)
        {
            _cliContext = cliContext;
            _webHostRunnerService = webHostRunnerService;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (_cliContext.IsInteractive)
                throw new CommandException(exitCode: 0, showHelp: true);

            await _webHostRunnerService.RunAsync(console.GetCancellationToken());
        }
    }
}