using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command("param cmd2", Description = "Command using positional parameters")]
    public class SimpleParameterCommand : ICommand
    {
        [CommandParameter(0, Name = "first")]
        public string? ParameterA { get; set; }

        [CommandParameter(10)]
        public int? ParameterB { get; set; }

        [CommandOption("option", 'o')]
        public string OptionA { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}