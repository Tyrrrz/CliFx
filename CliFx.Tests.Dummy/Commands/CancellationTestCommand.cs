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
            console.Output.WriteLine("Started.");

            await Task.Delay(
                TimeSpan.FromSeconds(3),
                console.RegisterCancellationHandler()
            );

            console.Output.WriteLine("Completed.");
        }
        catch (OperationCanceledException)
        {
            console.Output.WriteLine("Cancelled.");
            throw;
        }
    }
}