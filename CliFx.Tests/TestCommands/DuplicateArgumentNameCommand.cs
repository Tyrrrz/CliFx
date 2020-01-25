using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class DuplicateArgumentNameCommand : ICommand
    {
        [CommandParameter(0, Name = "Duplicate")]
        public string? ArgumentA { get; set; }

        [CommandParameter(1, Name = "Duplicate")]
        public string? ArgumentB { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}