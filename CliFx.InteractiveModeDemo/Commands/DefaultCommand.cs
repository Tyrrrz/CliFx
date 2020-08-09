using System;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.InteractiveModeDemo.Commands
{
    [Command]
    public class DefaultCommand : ICommand
    {
        public DefaultCommand()
        {

        }

        public ValueTask ExecuteAsync(IConsole console)
        {
            console.WithForegroundColor(ConsoleColor.DarkGreen, () => console.Output.WriteLine("Hello world from default command"));

            return default;
        }
    }
}