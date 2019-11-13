using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command(Description = "HelpDefaultCommand description.")]
    public class HelpDefaultCommand : ICommand
    {
        [CommandOption("option-a", 'a', Description = "OptionA description.")]
        public string? OptionA { get; set; }

        [CommandOption("option-b", 'b', Description = "OptionB description.")]
        public string? OptionB { get; set; }

        public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
    }
}