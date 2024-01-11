using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using CliWrap;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class DirectivesSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact(Timeout = 15000)]
    public async Task I_can_use_the_debug_directive_to_make_the_application_wait_for_the_debugger_to_attach()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // We can't actually attach a debugger, but we can ensure that the process is waiting for one
        void HandleStdOut(string line)
        {
            // Kill the process once it writes the output we expect
            if (line.Contains("Attach the debugger to", StringComparison.OrdinalIgnoreCase))
                cts.Cancel();
        }

        var command = Cli.Wrap(Dummy.Program.FilePath).WithArguments("[debug]") | HandleStdOut;

        // Act & assert
        try
        {
            await command.ExecuteAsync(cts.Token);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
        {
            // This means that the process was killed after it wrote the expected output
        }
    }

    [Fact]
    public async Task I_can_use_the_preview_directive_to_make_the_application_print_the_parsed_command_input()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
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
            ["[preview]", "cmd", "param", "-abc", "--option", "foo"],
            new Dictionary<string, string> { ["ENV_QOP"] = "hello", ["ENV_KIL"] = "world" }
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut
            .Should()
            .ContainAllInOrder(
                "cmd",
                "<param>",
                "[-a]",
                "[-b]",
                "[-c]",
                "[--option \"foo\"]",
                "ENV_QOP",
                "=",
                "\"hello\"",
                "ENV_KIL",
                "=",
                "\"world\""
            );
    }
}
