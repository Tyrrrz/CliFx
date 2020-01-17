using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("somerequired", Description = "SomeRequiredOptionsCommand description.")]
    public class SomeRequiredOptionsCommand : ICommand
    {
        [CommandOption("option-f", 'f', IsRequired = true, Description = "OptionF description.")]
        public string? OptionF { get; set; }

        [CommandOption("option-g", 'g', Description = "OptionG description.")]
        public string? OptionFG { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}
