using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class DuplicateParameterOrderCommand : ICommand
    {
        [CommandParameter(13)]
        public string? ArgumentA { get; set; }

        [CommandParameter(13)]
        public string? ArgumentB { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}