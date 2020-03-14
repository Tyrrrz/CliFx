using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public class EnvironmentVariablesSpecs
    {
        [Fact]
        public async Task Option_can_use_a_specific_environment_variable_as_fallback()
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

        [Fact]
        public async Task Option_only_uses_environment_variable_as_fallback_if_the_value_was_not_directly_provided()
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
    }
}