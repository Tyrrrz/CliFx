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

public class EnvironmentSpecs : SpecsBase
{
    public EnvironmentSpecs(ITestOutputHelper testOutput)
        : base(testOutput)
    {
    }

    [Fact]
    public async Task Option_can_fall_back_to_an_environment_variable()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", IsRequired = true, EnvironmentVariable = ""ENV_FOO"")]
    public string Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo);
        return default;
    }
}
");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>
            {
                ["ENV_FOO"] = "bar"
            }
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Trim().Should().Be("bar");
    }

    [Fact]
    public async Task Option_does_not_fall_back_to_an_environment_variable_if_a_value_is_provided_through_arguments()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", EnvironmentVariable = ""ENV_FOO"")]
    public string Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo);
        return default;
    }
}
");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            new[] {"--foo", "baz"},
            new Dictionary<string, string>
            {
                ["ENV_FOO"] = "bar"
            }
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Trim().Should().Be("baz");
    }

    [Fact]
    public async Task Option_of_non_scalar_type_can_receive_multiple_values_from_an_environment_variable()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", EnvironmentVariable = ""ENV_FOO"")]
    public IReadOnlyList<string> Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        foreach (var i in Foo)
            console.Output.WriteLine(i);

        return default;
    }
}
");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>
            {
                ["ENV_FOO"] = $"bar{Path.PathSeparator}baz"
            }
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Should().ConsistOfLines(
            "bar",
            "baz"
        );
    }

    [Fact]
    public async Task Option_of_scalar_type_always_receives_a_single_value_from_an_environment_variable()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", EnvironmentVariable = ""ENV_FOO"")]
    public string Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo);
        return default;
    }
}
");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>
            {
                ["ENV_FOO"] = $"bar{Path.PathSeparator}baz"
            }
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Trim().Should().Be($"bar{Path.PathSeparator}baz");
    }

    [Fact]
    public async Task Environment_variables_are_matched_case_sensitively()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", EnvironmentVariable = ""ENV_FOO"")]
    public string Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo);
        return default;
    }
}
");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>
            {
                ["ENV_foo"] = "baz",
                ["ENV_FOO"] = "bar",
                ["env_FOO"] = "qop"
            }
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Trim().Should().Be("bar");
    }

    [Fact]
    public async Task Environment_variables_are_extracted_automatically()
    {
        // Ensures that the environment variables are properly obtained from
        // System.Environment when they are not provided explicitly to CliApplication.

        // Arrange
        var command = Cli.Wrap("dotnet")
            .WithArguments(a => a
                .Add(Dummy.Program.Location)
                .Add("env-test"))
            .WithEnvironmentVariables(e => e
                .Set("ENV_TARGET", "Mars"));

        // Act
        var result = await command.ExecuteBufferedAsync();

        // Assert
        result.StandardOutput.Trim().Should().Be("Hello Mars!");
    }
}