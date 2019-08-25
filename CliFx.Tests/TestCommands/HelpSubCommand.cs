using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("cmd sub", Description = "HelpSubCommand description.")]
    public class HelpSubCommand : ICommand
    {
        [CommandOption("option-e", 'e', Description = "OptionE description.")]
        public string OptionE { get; set; }

        public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
    }
}