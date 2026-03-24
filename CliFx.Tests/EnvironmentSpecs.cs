using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using CliWrap;
using CliWrap.Buffered;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class EnvironmentSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public async Task I_can_configure_an_option_to_fall_back_to_an_environment_variable_if_the_user_does_not_provide_the_corresponding_argument()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo", EnvironmentVariable = "ENV_FOO")]
                public string? Foo { get; set; }

                [CommandOption("bar", EnvironmentVariable = "ENV_BAR")]
                public string? Bar { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    console.WriteLine(Bar);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["--foo", "42"],
            new Dictionary<string, string> { ["ENV_FOO"] = "100", ["ENV_BAR"] = "200" }
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().ConsistOfLines("42", "200");
    }

    [Fact]
    public async Task I_can_configure_an_option_bound_to_a_non_scalar_property_to_fall_back_to_an_environment_variable_if_the_user_does_not_provide_the_corresponding_argument()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo", EnvironmentVariable = "ENV_FOO")]
                public IReadOnlyList<string>? Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            [],
            new Dictionary<string, string> { ["ENV_FOO"] = $"bar{Path.PathSeparator}baz" }
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("bar", "baz");
    }

    [Fact]
    public async Task I_can_configure_an_option_bound_to_a_scalar_property_to_fall_back_to_an_environment_variable_while_ignoring_path_separators()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo", EnvironmentVariable = "ENV_FOO")]
                public string? Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            [],
            new Dictionary<string, string> { ["ENV_FOO"] = $"bar{Path.PathSeparator}baz" }
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be($"bar{Path.PathSeparator}baz");
    }

    [Fact(Timeout = 15000)]
    public async Task I_can_run_the_application_and_it_will_resolve_all_required_environment_variables_automatically()
    {
        // Ensures that the environment variables are properly obtained from
        // System.Environment when they are not provided explicitly to CommandLineApplication.

        // Arrange
        var command = Cli.Wrap(Dummy.Program.FilePath)
            .WithArguments("env-test")
            .WithEnvironmentVariables(e => e.Set("ENV_TARGET", "Mars"));

        // Act
        var result = await command.ExecuteBufferedAsync();

        // Assert
        result.StandardOutput.Trim().Should().Be("Hello Mars!");
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
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command("cmd")]
            public partial class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
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
