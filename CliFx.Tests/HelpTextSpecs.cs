using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class HelpTextSpecs : SpecsBase
    {
        public HelpTextSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Help_text_shows_command_usage_format_which_lists_all_parameters()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<WithParametersCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAllInOrder(
                "Usage",
                "cmd", "<parama>", "<paramb>", "<paramc...>"
            );
        }

        [Fact]
        public async Task Help_text_shows_command_usage_format_which_lists_all_required_options()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<WithRequiredOptionsCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAllInOrder(
                "Usage",
                "cmd", "--opt-a <value>", "--opt-c <values...>", "[options]",
                "Options",
                "*", "-a|--opt-a",
                "*", "-c|--opt-c",
                "-b|--opt-b"
            );
        }

        [Fact]
        public async Task Help_text_shows_usage_format_which_lists_all_available_sub_commands()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAllInOrder(
                "Usage",
                "... named",
                "... named sub"
            );
        }

        [Fact]
        public async Task Help_text_shows_all_valid_values_for_enum_arguments()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<WithEnumArgumentsCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAllInOrder(
                "Parameters",
                "enum", "Valid values:", "Value1", "Value2", "Value3",
                "Options",
                "--required-enum", "Valid values:", "Value1", "Value2", "Value3",
                "--enum", "Valid values:", "Value1", "Value2", "Value3"
            );
        }

        [Fact]
        public async Task Help_text_shows_environment_variables_for_options_that_have_them_configured_as_fallback()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<WithEnvironmentVariablesCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAllInOrder(
                "Options",
                "-a|--opt-a", "Environment variable:", "ENV_OPT_A",
                "-b|--opt-b", "Environment variable:", "ENV_OPT_B"
            );
        }

        [Fact]
        public async Task Help_text_shows_default_values_for_non_required_options()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<WithDefaultValuesCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAllInOrder(
                "Options",
                "--obj", "Default:", "42",
                "--str", "Default:", "foo",
                "--str-empty", "Default:", "",
                "--str-array", "Default:", "foo", "bar", "baz",
                "--bool", "Default:", "True",
                "--char", "Default:", "t",
                "--int", "Default:", "1337",
                "--int-nullable", "Default:", "1337",
                "--int-array", "Default:", "1", "2", "3",
                "--timespan", "Default:", "02:03:00",
                "--enum", "Default:", "Value2"
            );
        }
    }
}