using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.BlazorDemo.CLI.Services;

namespace CliFx.BlazorDemo.CLI.Commands
{
    [Command("webhost start", Description = "Starts the webhost worker in background in the interactive mode.", InteractiveModeOnly = true)]
    public class WebHostStartCommand : ICommand
    {
        private readonly IBackgroundWebHostProvider _webHostProvider;

        public WebHostStartCommand(IBackgroundWebHostProvider webHostProvider)
        {
            _webHostProvider = webHostProvider;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            await _webHostProvider.StartAsync();
        }
    }
}
