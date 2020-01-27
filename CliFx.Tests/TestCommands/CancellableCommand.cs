using System;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command("cancel")]
    public class CancellableCommand : ICommand
    {
        public async ValueTask ExecuteAsync(IConsole console)
        {
            await Task.Delay(TimeSpan.FromSeconds(3), console.GetCancellationToken());
            console.Output.WriteLine("Never printed");
        }
    }
}