using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Benchmarks.Commands
{
    [Command]
    public class CliFxCommand : ICommand
    {
        [CommandOption("str", 's')]
        public string StrOption { get; set; }

        [CommandOption("int", 'i')]
        public int IntOption { get; set; }

        [CommandOption("bool", 'b')]
        public bool BoolOption { get; set; }

        public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
    }
}
