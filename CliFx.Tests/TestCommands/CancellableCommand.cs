using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("cancel")]
    public class CancellableCommand : ICommand
    {
        public async Task ExecuteAsync(IConsole console)
        {
            await Task.Yield();

            console.Output.WriteLine("Printed");

            await Task.Delay(TimeSpan.FromSeconds(1), console.GetCancellationToken()).ConfigureAwait(false);

            console.Output.WriteLine("Never printed");
        }
    }
}
