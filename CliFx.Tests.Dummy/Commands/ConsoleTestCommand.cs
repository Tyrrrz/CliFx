using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace CliFx.Tests.Dummy.Commands;

[Command("console-test")]
public class ConsoleTestCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        var input = console.Input.ReadToEnd();

        using (console.WithColors(ConsoleColor.Black, ConsoleColor.White))
        {
            console.Output.WriteLine(input);
            console.Error.WriteLine(input);
        }

        return default;
    }
}
