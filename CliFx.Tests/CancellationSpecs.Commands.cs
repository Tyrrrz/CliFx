namespace CliFx.Tests
{
    using System;
    using System.Threading.Tasks;
    using CliFx.Attributes;

    public partial class CancellationSpecs
    {
        [Command("cancel")]
        private class CancellableCommand : ICommand
        {
            public async ValueTask ExecuteAsync(IConsole console)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), console.GetCancellationToken());
                    console.Output.WriteLine("Never printed");
                }
                catch (OperationCanceledException)
                {
                    console.Output.WriteLine("Cancellation requested");
                    throw;
                }
            }
        }
    }
}