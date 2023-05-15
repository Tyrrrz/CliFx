using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
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
        var stdOutBuffer = new StringBuilder();

        var command = Cli.Wrap("dotnet")
            .WithArguments(a => a
                .Add(Dummy.Program.Location)
                .Add("cancel-test")
            ) | stdOutBuffer;

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(0.2));

        // Act & assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await command.ExecuteAsync(
                // Forceful cancellation (not required because we have a timeout)
                CancellationToken.None,
                // Graceful cancellation
                cts.Token
            )
        );

        stdOutBuffer.ToString().Trim().Should().Be("Cancelled");
    }

    [Fact]
    public async Task I_can_configure_the_command_to_listen_to_the_interrupt_signal_when_running_in_isolation()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                public async ValueTask ExecuteAsync(IConsole console)
                {
                    try
                    {
                        await Task.Delay(
                            TimeSpan.FromSeconds(3),
                            console.RegisterCancellationHandler()
                        );

                        console.Output.WriteLine("Completed successfully");
                    }
                    catch (OperationCanceledException)
                    {
                        console.Output.WriteLine("Cancelled");
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
        stdOut.Trim().Should().Be("Cancelled");
    }
}