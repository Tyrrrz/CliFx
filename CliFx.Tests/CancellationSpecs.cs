using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
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

    [Fact]
    public async Task Command_can_register_to_receive_a_cancellation_signal_from_the_console()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
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

            console.Output.WriteLine(""Completed successfully"");
        }
        catch (OperationCanceledException)
        {
            console.Output.WriteLine(""Cancelled"");
            throw;
        }
    }
}");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        FakeConsole.RequestCancellation(TimeSpan.FromSeconds(0.2));

        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().NotBe(0);
        stdOut.Trim().Should().Be("Cancelled");
    }
}