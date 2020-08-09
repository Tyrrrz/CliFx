using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.BlazorDemo.CLI.Services;

namespace CliFx.BlazorDemo.CLI.Commands
{
    [Command("webhost status", Description = "Returns webhost worker status in the interactive mode.", InteractiveModeOnly = true)]
    public class WebHostStatusCommand : ICommand
    {
        private readonly ICliContext _cliContex;
        private readonly IBackgroundWebHostProvider _webHostProvider;

        public WebHostStatusCommand(ICliContext cliContext, IBackgroundWebHostProvider webHostProvider)
        {
            _cliContex = cliContext;
            _webHostProvider = webHostProvider;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            await console.Output.WriteLineAsync(_cliContex.Metadata.Title);
            await console.Output.WriteLineAsync($"Status: {_webHostProvider.Status}");
            await console.Output.WriteLineAsync($"Startup time: {_webHostProvider.StartupTime}");
            await console.Output.WriteLineAsync($"Runtime: {_webHostProvider.Runtime}");
        }
    }
}
