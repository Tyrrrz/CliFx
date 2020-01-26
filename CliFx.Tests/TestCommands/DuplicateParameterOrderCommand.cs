using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class DuplicateParameterOrderCommand : ICommand
    {
        [CommandParameter(13)]
        public string? ParameterA { get; set; }

        [CommandParameter(13)]
        public string? ParameterB { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}