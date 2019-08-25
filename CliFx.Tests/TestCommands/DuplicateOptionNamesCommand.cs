using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class DuplicateOptionNamesCommand : ICommand
    {
        [CommandOption("fruits")]
        public string Apples { get; set; }
        
        [CommandOption("fruits")]
        public string Oranges { get; set; }
        
        public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
    }
}