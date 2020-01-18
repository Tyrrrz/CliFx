using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("arg cmd2", Description = "Command using positional arguments")]
    public class SimpleArgumentCommand : ICommand
    {
        [CommandParameter(0, Name = "first")]
        public string? FirstArgument { get; set; }

        [CommandParameter(10)]
        public int? SecondArgument { get; set; }

        [CommandOption("option", 'o')]
        public string Option { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}