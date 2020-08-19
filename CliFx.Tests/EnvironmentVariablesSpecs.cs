using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
using CliWrap;
using CliWrap.Buffered;
using FluentAssertions;
using Newtonsoft.Json;
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
            stdOut.TrimEnd().Should().Be("Hello Mars!");
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
            stdOut.TrimEnd().Should().Be("Hello Jupiter!");
        }

        [Fact]
        public async Task Option_only_uses_an_environment_variable_as_fallback_if_the_name_matches_case_sensitively()
        {
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

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

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();
            var commandInstance = JsonConvert.DeserializeObject<WithEnvironmentVariablesCommand>(stdOutData);

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
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

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

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();
            var commandInstance = JsonConvert.DeserializeObject<WithEnvironmentVariablesCommand>(stdOutData);

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
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

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

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();
            var commandInstance = JsonConvert.DeserializeObject<WithEnvironmentVariablesCommand>(stdOutData);

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new WithEnvironmentVariablesCommand
            {
                OptA = $"foo{Path.PathSeparator}bar"
            });
        }
    }
}