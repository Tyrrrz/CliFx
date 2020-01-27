using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command("cmd sub", Description = "HelpSubCommand description.")]
    public class HelpSubCommand : ICommand
    {
        [CommandOption("option-e", 'e', Description = "OptionE description.")]
        public string? OptionE { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}