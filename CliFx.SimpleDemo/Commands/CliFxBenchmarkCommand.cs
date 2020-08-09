using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.SimpleDemo.Commands
{
    [Command]
    public class CliFxBenchmarkCommand : ICommand
    {
        [CommandOption("str", 's')]
        public string? StrOption { get; set; }

        [CommandOption("int", 'i')]
        public int IntOption { get; set; }

        [CommandOption("bool", 'b')]
        public bool BoolOption { get; set; }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            await console.Output.WriteLineAsync($"Hello world {StrOption} {IntOption} {BoolOption}");
        }
    }
}