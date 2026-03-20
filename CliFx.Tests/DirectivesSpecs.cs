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
    public async Task I_can_use_an_environment_variable_to_make_the_application_wait_for_the_debugger_to_attach()
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

        var command =
            Cli.Wrap(Dummy.Program.FilePath)
                .WithEnvironmentVariables(e => e.Set("CLIFX_DEBUG", "1")) | HandleStdOut;

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
    public async Task I_can_use_an_environment_variable_to_make_the_application_print_the_parsed_command_input()
    {
        // Arrange
        var commandDescriptor = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command("cmd")]
            public partial class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandDescriptor)
            .UseConsole(FakeConsole)
            .AllowPreviewMode("CLIFX_PREVIEW")
            .Build();

        // Act
        await application.RunAsync(
            // Above command doesn't support these inputs, so the exit code
            // will be non-zero, but it's not relevant for this test.
            ["cmd", "param", "-abc", "--option", "foo"],
            new Dictionary<string, string> { ["CLIFX_PREVIEW"] = "true" }
        );

        // Assert
        var stdOut = FakeConsole.ReadOutputString();
        stdOut
            .Should()
            .ContainAllInOrder("cmd", "<param>", "[-a]", "[-b]", "[-c]", "[--option \"foo\"]");
    }
}
