using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class DuplicateArgumentOrderCommand : ICommand
    {
        [CommandParameter(13)]
        public string? ArgumentA { get; set; }

        [CommandParameter(13)]
        public string? ArgumentB { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}