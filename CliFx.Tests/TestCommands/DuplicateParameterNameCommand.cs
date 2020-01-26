using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class DuplicateParameterNameCommand : ICommand
    {
        [CommandParameter(0, Name = "Duplicate")]
        public string? ParameterA { get; set; }

        [CommandParameter(1, Name = "Duplicate")]
        public string? ParameterB { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}