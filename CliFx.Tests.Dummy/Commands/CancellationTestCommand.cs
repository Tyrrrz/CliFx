using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace CliFx.Tests.Dummy.Commands;

[Command("cancel-test")]
public class CancellationTestCommand : ICommand
{
    public async ValueTask ExecuteAsync(IConsole console)
    {
        try
        {
            console.WriteLine("Started.");

            await Task.Delay(TimeSpan.FromSeconds(3), console.RegisterCancellationHandler());

            console.WriteLine("Completed.");
        }
        catch (OperationCanceledException)
        {
            console.WriteLine("Cancelled.");
            throw;
        }
    }
}
