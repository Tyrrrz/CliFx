using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class DuplicateOptionShortNamesCommand : ICommand
    {
        [CommandOption('f')]
        public string Apples { get; set; }
        
        [CommandOption('f')]
        public string Oranges { get; set; }
        
        public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
    }
}