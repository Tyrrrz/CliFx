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

    [Fact(Timeout = 15000)]
    public async Task Debug_directive_can_be_specified_to_interrupt_execution_until_a_debugger_is_attached()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // We can't actually attach a debugger, but we can ensure that the process is waiting for one
        void HandleStdOut(string line)
        {
            // Kill the process once it writes the output we expect
            if (line.Contains("Attach debugger to", StringComparison.OrdinalIgnoreCase))
                cts.Cancel();
        }

        var command = Cli.Wrap("dotnet")
            .WithArguments(a => a
                .Add(Dummy.Program.Location)
                .Add("[debug]")
            ) | HandleStdOut;

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