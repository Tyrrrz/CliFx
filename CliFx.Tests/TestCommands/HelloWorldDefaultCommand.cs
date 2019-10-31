using System.Threading;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class HelloWorldDefaultCommand : ICommand
    {
        public Task ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine("Hello world.");
            return Task.CompletedTask;
        }
    }
}