using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.InteractiveModeDemo.Commands
{
    [Command]
    public class DefaultCommand : ICommand
    {
        [CommandParameter(0)]
        public IReadOnlyList<string> Values { get; set; }

        public DefaultCommand()
        {

        }

        public ValueTask ExecuteAsync(IConsole console)
        {
            console.WithForegroundColor(ConsoleColor.DarkGreen, () => console.Output.WriteLine("Hello world from default command"));
            foreach (var value in Values)
            {
                console.Output.WriteLine(value);
            }

            return default;
        }
    }
}