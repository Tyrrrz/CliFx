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

namespace CliFx.Tests
{
    public class EnvironmentVariablesSpecs : SpecsBase
    {
        public EnvironmentVariablesSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        // This test uses a real application to make sure environment variables are actually read correctly
        [Fact]
        public async Task Option_can_fall_back_to_an_environment_variable()
        {
            // Arrange
            var command = Cli.Wrap("dotnet")
                .WithArguments(a => a
                    .Add(Dummy.Program.Location))
                .WithEnvironmentVariables(e => e
                    .Set("ENV_TARGET", "Mars"));

            // Act
            var stdOut = await command.ExecuteBufferedAsync().Select(r => r.StandardOutput);

            // Assert
            stdOut.Trim().Should().Be("Hello Mars!");
        }

        // This test uses a real application to make sure environment variables are actually read correctly
        [Fact]
        public async Task Option_does_not_fall_back_to_an_environment_variable_if_the_value_is_provided_through_arguments()
        {
            // Arrange
            var command = Cli.Wrap("dotnet")
                .WithArguments(a => a
                    .Add(Dummy.Program.Location)
                    .Add("--target")
                    .Add("Jupiter"))
                .WithEnvironmentVariables(e => e
                    .Set("ENV_TARGET", "Mars"));

            // Act
            var stdOut = await command.ExecuteBufferedAsync().Select(r => r.StandardOutput);

            // Assert
            stdOut.Trim().Should().Be("Hello Jupiter!");
        }

        [Fact]
        public async Task Option_of_non_scalar_type_relies_on_the_path_separator_to_extract_multiple_values()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", EnvironmentVariable = ""ENV_FOO"")]
    public IReadOnlyList<string>? Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console)
    {
        foreach (var i in Foo)
        {
            console.Output.WriteLine(i);
        }
        
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
        public async Task Option_of_scalar_type_ignores_path_separators()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", EnvironmentVariable = ""ENV_FOO"")]
    public string? Foo { get; set; }
    
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
    public string? Foo { get; set; }
    
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
    }
}