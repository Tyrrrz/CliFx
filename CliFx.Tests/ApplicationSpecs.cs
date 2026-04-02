using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using CliWrap;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class ApplicationSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public async Task I_can_create_an_application_with_the_default_configuration()
    {
        // Act
        var app = new CommandLineApplicationBuilder().AddCommandsFromThisAssembly().Build();

        var exitCode = await app.RunAsync([], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task I_can_create_an_application_with_a_custom_configuration()
    {
        // Act
        var app = new CommandLineApplicationBuilder()
            .SetTitle("My App")
            .SetVersion("1.0.0")
            .SetDescription("This is my app.")
            .SetExecutableName("myapp")
            .AddCommand(NoOpCommand.Descriptor)
            .AddCommands([NoOpCommand.Descriptor])
            .AllowDebugMode()
            .AllowPreviewMode()
            .UseConsole(FakeConsole)
            .UseTypeActivator(new DefaultTypeActivator())
            .Build();

        var exitCode = await app.RunAsync([], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public void I_can_create_an_application_without_a_configuration()
    {
        // Act
        var commands = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """,
            OutputKind.ConsoleApplication
        );

        // Assert
        commands[0].Type.Assembly.EntryPoint.Should().NotBeNull();
    }

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
                .WithEnvironmentVariables(e => e.Set("CLIFX_DEBUG", "true")) | HandleStdOut;

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
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command("cmd")]
                    public partial class Command : ICommand
                    {
                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .AllowPreviewMode()
            .Build();

        // Act
        await application.RunAsync(
            // Above command doesn't support these inputs, so the exit code
            // will be non-zero, but it's not relevant to this test.
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
