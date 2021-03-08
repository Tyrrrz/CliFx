using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils;
using CliWrap;
using CliWrap.Buffered;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public class EnvironmentVariablesSpecs
    {
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
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<WithEnvironmentVariablesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd"},
                new Dictionary<string, string>
                {
                    ["ENV_OPT_B"] = $"foo{Path.PathSeparator}bar"
                }
            );

            var commandInstance = console.ReadOutputString().DeserializeJson<WithEnvironmentVariablesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new WithEnvironmentVariablesCommand
            {
                OptB = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Option_of_scalar_type_ignores_path_separators()
        {
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<WithEnvironmentVariablesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd"},
                new Dictionary<string, string>
                {
                    ["ENV_OPT_A"] = $"foo{Path.PathSeparator}bar"
                }
            );

            var commandInstance = console.ReadOutputString().DeserializeJson<WithEnvironmentVariablesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new WithEnvironmentVariablesCommand
            {
                OptA = $"foo{Path.PathSeparator}bar"
            });
        }

        [Fact]
        public async Task Environment_variables_are_matched_case_sensitively()
        {
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<WithEnvironmentVariablesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd"},
                new Dictionary<string, string>
                {
                    ["ENV_opt_A"] = "incorrect",
                    ["ENV_OPT_A"] = "correct"
                }
            );

            var commandInstance = console.ReadOutputString().DeserializeJson<WithEnvironmentVariablesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new WithEnvironmentVariablesCommand
            {
                OptA = "correct"
            });
        }
    }
}