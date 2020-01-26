using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class DuplicateOptionShortNamesCommand : ICommand
    {
        [CommandOption('x')]
        public string? OptionA { get; set; }

        [CommandOption('x')]
        public string? OptionB { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}