using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("sampleval", Description = "SampleValueOptionCommand description.")]
    public class SampleValueOptionCommand : ICommand
    {
        [CommandOption("option-f", 'f', IsRequired = true, Description = "OptionF description.", SampleValue = "option_f_value")]
        public string? OptionF { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}
