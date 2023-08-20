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

public class CancellationSpecs : SpecsBase
{
    public CancellationSpecs(ITestOutputHelper testOutput)
        : base(testOutput)
    {
    }

    [Fact(Timeout = 15000)]
    public async Task I_can_configure_the_command_to_listen_to_the_interrupt_signal()
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

        var pipeTarget = PipeTarget.Merge(
            PipeTarget.ToDelegate(HandleStdOut),
            PipeTarget.ToStringBuilder(stdOutBuffer)
        );

        var command =
            Cli.Wrap(Dummy.Program.FilePath).WithArguments("cancel-test") |
            pipeTarget;

        // Act & assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await command.ExecuteAsync(
                // Forceful cancellation (not required because we have a timeout)
                CancellationToken.None,
                // Graceful cancellation
                cts.Token
            )
        );

        stdOutBuffer.ToString().Trim().Should().ConsistOfLines(
            "Started.",
            "Cancelled."
        );
    }

    [Fact]
    public async Task I_can_configure_the_command_to_listen_to_the_interrupt_signal_when_running_in_isolation()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
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
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        FakeConsole.RequestCancellation(TimeSpan.FromSeconds(0.2));

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().NotBe(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().ConsistOfLines(
            "Started.",
            "Cancelled."
        );
    }
}