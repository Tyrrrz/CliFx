using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using CliWrap;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class CancellationSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact(Timeout = 15000)]
    public async Task I_can_listen_to_the_interrupt_signal()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // We need to send the cancellation request right after the process has registered
        // a handler for the interrupt signal, otherwise the default handler will trigger
        // and just kill the process.
        void HandleStdOut(string line)
        {
            if (string.Equals(line, "Started.", StringComparison.OrdinalIgnoreCase))
                cts.CancelAfter(TimeSpan.FromSeconds(0.2));
        }

        var stdOutBuffer = new StringBuilder();

        var command =
            Cli.Wrap(Dummy.Program.FilePath).WithArguments("cancel-test")
            | PipeTarget.Merge(
                PipeTarget.ToDelegate(HandleStdOut),
                PipeTarget.ToStringBuilder(stdOutBuffer)
            );

        // Act
        var act = async () =>
            await command.ExecuteAsync(
                // Forceful cancellation (not required because we have a timeout)
                CancellationToken.None,
                // Graceful cancellation
                cts.Token
            );

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();

        stdOutBuffer.ToString().Trim().Should().ConsistOfLines("Started.", "Cancelled.");
    }

    [Fact]
    public async Task I_can_listen_to_the_interrupt_signal_when_running_against_a_fake_console()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                public async ValueTask ExecuteAsync(IConsole console)
                {
                    try
                    {
                        console.WriteLine("Started.");

                        await Task.Delay(
                            TimeSpan.FromSeconds(3),
                            console.RegisterCancellationHandler()
                        );

                        console.WriteLine("Completed.");
                    }
                    catch (OperationCanceledException)
                    {
                        console.WriteLine("Cancelled.");
                        throw;
                    }
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        FakeConsole.RequestCancellation(TimeSpan.FromSeconds(0.2));

        // Act
        var exitCode = await application.RunAsync([], new Dictionary<string, string>());

        // Assert
        exitCode.Should().NotBe(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().ConsistOfLines("Started.", "Cancelled.");
    }
}
