using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("allrequired", Description = "AllRequiredOptionsCommand description.")]
    public class AllRequiredOptionsCommand : ICommand
    {
        [CommandOption("option-f", 'f', IsRequired = true, Description = "OptionF description.")]
        public string? OptionF { get; set; }

        [CommandOption("option-g", 'g', IsRequired = true, Description = "OptionG description.")]
        public string? OptionFG { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}
