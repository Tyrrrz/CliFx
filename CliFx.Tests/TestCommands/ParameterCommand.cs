using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command("param cmd", Description = "Command using positional parameters")]
    public class ParameterCommand : ICommand
    {
        [CommandParameter(0, Name = "first")]
        public string? FirstArgument { get; set; }

        [CommandParameter(10)]
        public int? SecondArgument { get; set; }

        [CommandParameter(20, Description = "A list of numbers", Name = "third list")]
        public IEnumerable<int> ThirdArguments { get; set; }

        [CommandOption("option", 'o')]
        public string Option { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}