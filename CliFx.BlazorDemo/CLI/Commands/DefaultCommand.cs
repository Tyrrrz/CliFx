using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.BlazorDemo.CLI.Services;

namespace CliFx.BlazorDemo.CLI.Commands
{
    [Command(Description = "Runs webhost in normal mode.")]
    public class RunWebHostCommand : ICommand
    {
        private readonly IWebHostRunnerService _webHostRunnerService;

        public RunWebHostCommand(IWebHostRunnerService webHostRunnerService)
        {
            _webHostRunnerService = webHostRunnerService;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            await _webHostRunnerService.RunAsync(console.GetCancellationToken());
        }
    }
}
