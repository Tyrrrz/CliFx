using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.BlazorDemo.CLI.Services;
using CliFx.Exceptions;

namespace CliFx.BlazorDemo.CLI.Commands
{
    [Command("webhost restart", Description = "Restarts the webhost worker in the interactive mode.", InteractiveModeOnly = true)]
    public class WebHostRestartCommand : ICommand
    {
        private readonly IBackgroundWebHostProvider _webHostProvider;

        public WebHostRestartCommand(IBackgroundWebHostProvider webHostProvider)
        {
            _webHostProvider = webHostProvider;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (_webHostProvider.Status == WebHostStatuses.Stopped)
                throw new CommandException("WebHost is stopped.");

            await _webHostProvider.RestartAsync(console.GetCancellationToken());
        }
    }
}
