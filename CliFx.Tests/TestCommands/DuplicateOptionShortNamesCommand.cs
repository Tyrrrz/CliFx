using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class DuplicateOptionShortNamesCommand : ICommand
    {
        [CommandOption('f')]
        public string? Apples { get; set; }
        
        [CommandOption('f')]
        public string? Oranges { get; set; }
        
        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}