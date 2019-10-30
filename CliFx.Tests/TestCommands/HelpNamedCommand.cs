using System.Threading;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("cmd", Description = "HelpNamedCommand description.")]
    public class HelpNamedCommand : ICommand
    {
        [CommandOption("option-c", 'c', Description = "OptionC description.")]
        public string OptionC { get; set; }

        [CommandOption("option-d", 'd', Description = "OptionD description.")]
        public string OptionD { get; set; }

        public Task ExecuteAsync(IConsole console, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}