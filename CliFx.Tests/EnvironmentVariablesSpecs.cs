using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
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
        public async Task Option_can_use_an_environment_variable_as_fallback()
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
        public async Task Option_only_uses_an_environment_variable_as_fallback_if_the_value_is_not_directly_provided()
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
        public async Task Option_only_uses_an_environment_variable_as_fallback_if_the_name_matches_case_sensitively()
        {
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

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

            var commandInstance = stdOut.GetString().DeserializeJson<WithEnvironmentVariablesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new WithEnvironmentVariablesCommand
            {
                OptA = "correct"
            });
        }

        [Fact]
        public async Task Option_of_non_scalar_type_can_use_an_environment_variable_as_fallback_and_extract_multiple_values()
        {
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

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

            var commandInstance = stdOut.GetString().DeserializeJson<WithEnvironmentVariablesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new WithEnvironmentVariablesCommand
            {
                OptB = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public async Task Option_of_scalar_type_can_use_an_environment_variable_as_fallback_regardless_of_separators()
        {
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

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

            var commandInstance = stdOut.GetString().DeserializeJson<WithEnvironmentVariablesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new WithEnvironmentVariablesCommand
            {
                OptA = $"foo{Path.PathSeparator}bar"
            });
        }
    }
}