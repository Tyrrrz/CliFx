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

public class DirectivesSpecs : SpecsBase
{
    public DirectivesSpecs(ITestOutputHelper testOutput)
        : base(testOutput)
    {
    }

    [Fact]
    public async Task Debug_directive_can_be_specified_to_interrupt_execution_until_a_debugger_is_attached()
    {
        // Arrange
        var stdOutBuffer = new StringBuilder();

        var command = Cli.Wrap("dotnet")
            .WithArguments(a => a
                .Add(Dummy.Program.Location)
                .Add("[debug]")
            ) | stdOutBuffer;

        // Act
        try
        {
            // This has a timeout just in case the execution hangs forever
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var task = command.ExecuteAsync(cts.Token);

            // We can't attach a debugger programmatically, so the application
            // will hang indefinitely.
            // To work around it, we will wait until the application writes
            // something to the standard output and then kill it.
            while (true)
            {
                if (stdOutBuffer.Length > 0)
                {
                    cts.Cancel();
                    break;
                }

                await Task.Delay(100, cts.Token);
            }

            await task;
        }
        catch (OperationCanceledException)
        {
            // This is expected
        }

        var stdOut = stdOutBuffer.ToString();

        // Assert
        stdOut.Should().Contain("Attach debugger to");

        TestOutput.WriteLine(stdOut);
    }

    [Fact]
    public async Task Preview_directive_can_be_specified_to_print_command_input()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command("cmd")]
            public class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .AllowPreviewMode()
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            new[] {"[preview]", "cmd", "param", "-abc", "--option", "foo"},
            new Dictionary<string, string>
            {
                ["ENV_QOP"] = "hello",
                ["ENV_KIL"] = "world"
            }
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Should().ContainAllInOrder(
            "cmd", "<param>", "[-a]", "[-b]", "[-c]", "[--option \"foo\"]",
            "ENV_QOP", "=", "\"hello\"",
            "ENV_KIL", "=", "\"world\""
        );
    }
}