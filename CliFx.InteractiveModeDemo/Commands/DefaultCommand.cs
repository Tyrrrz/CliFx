using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.InteractiveModeDemo.Commands
{
    [Command]
    public class DefaultCommand : ICommand
    {
        private readonly ICliContext _cliContext;

        [CommandParameter(0)]
        public IReadOnlyList<string> Values { get; set; } = default!;

        public DefaultCommand(ICliContext cliContext)
        {
            _cliContext = cliContext;
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