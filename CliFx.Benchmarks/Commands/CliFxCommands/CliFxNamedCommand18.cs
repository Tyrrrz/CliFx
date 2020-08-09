using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Benchmarks.Commands.CliFxCommands
{
    [Command("named-command18")]
    public class CliFxNamedCommandCommand18 : ICommand
    {
        [CommandOption("str", 's')]
        public string? StrOption { get; set; }

        [CommandOption("int", 'i')]
        public int IntOption { get; set; }

        [CommandOption("bool", 'b')]
        public bool BoolOption { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}