using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CliFx.Domain;
using CliWrap;
using CliWrap.Buffered;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class EnvironmentVariablesSpecs
    {
        // This test uses a real application to make sure environment variables are actually read correctly
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

        // This test uses a real application to make sure environment variables are actually read correctly
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

        [Fact]
        public void Option_of_non_scalar_type_can_take_multiple_separated_values_from_an_environment_variable()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(EnvironmentVariableCollectionCommand)});

            var input = CommandLineInput.Empty;
            var envVars = new Dictionary<string, string>
            {
                ["ENV_OPT"] = $"foo{Path.PathSeparator}bar"
            };

            // Act
            var command = schema.InitializeEntryPoint(input, envVars);

            // Assert
            command.Should().BeEquivalentTo(new EnvironmentVariableCollectionCommand
            {
                Option = new[] {"foo", "bar"}
            });
        }

        [Fact]
        public void Option_of_scalar_type_can_only_take_a_single_value_from_an_environment_variable_even_if_it_contains_separators()
        {
            // Arrange
            var schema = ApplicationSchema.Resolve(new[] {typeof(EnvironmentVariableCommand)});

            var input = CommandLineInput.Empty;
            var envVars = new Dictionary<string, string>
            {
                ["ENV_OPT"] = $"foo{Path.PathSeparator}bar"
            };

            // Act
            var command = schema.InitializeEntryPoint(input, envVars);

            // Assert
            command.Should().BeEquivalentTo(new EnvironmentVariableCommand
            {
                Option = $"foo{Path.PathSeparator}bar"
            });
        }
    }
}