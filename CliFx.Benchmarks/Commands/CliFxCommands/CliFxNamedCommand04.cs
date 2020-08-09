using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Benchmarks.Commands.CliFxCommands
{
    [Command("named-command04")]
    public class CliFxNamedCommandCommand04 : ICommand
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