using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.BlazorDemo.CLI.Services;

namespace CliFx.BlazorDemo.CLI.Commands
{
    [Command("webhost stop", Description = "Stops the webhost background worker in the interactive mode.", InteractiveModeOnly = true)]
    public class WebHostStopCommand : ICommand
    {
        private readonly IBackgroundWebHostProvider _webHostProvider;

        public WebHostStopCommand(IBackgroundWebHostProvider webHostProvider)
        {
            _webHostProvider = webHostProvider;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            await _webHostProvider.StopAsync(console.GetCancellationToken());
        }
    }
}
