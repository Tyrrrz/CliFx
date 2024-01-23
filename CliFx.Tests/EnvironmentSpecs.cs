using System;
using System.Collections.Generic;
using System.IO;
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
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo", EnvironmentVariable = "ENV_FOO")]
                public string? Foo { get; init; }

                [CommandOption("bar", EnvironmentVariable = "ENV_BAR")]
                public string? Bar { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    console.WriteLine(Bar);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo", EnvironmentVariable = "ENV_FOO")]
                public IReadOnlyList<string>? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
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
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo", EnvironmentVariable = "ENV_FOO")]
                public string? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
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
        // System.Environment when they are not provided explicitly to CliApplication.

        // Arrange
        var command = Cli.Wrap(Dummy.Program.FilePath)
            .WithArguments("env-test")
            .WithEnvironmentVariables(e => e.Set("ENV_TARGET", "Mars"));

        // Act
        var result = await command.ExecuteBufferedAsync();

        // Assert
        result.StandardOutput.Trim().Should().Be("Hello Mars!");
    }
}
