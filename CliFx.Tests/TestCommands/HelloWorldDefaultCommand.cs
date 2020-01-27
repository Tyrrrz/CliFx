using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class HelloWorldDefaultCommand : ICommand
    {
        public ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine("Hello world.");
            return default;
        }
    }
}