using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("echo")]
    public class EchoCommand : ICommand
    {
        [CommandOption("message", 'm', IsRequired = true)]
        public string Message { get; set; }
        
        public Task ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine(Message);
            return Task.CompletedTask;
        }
    }
}