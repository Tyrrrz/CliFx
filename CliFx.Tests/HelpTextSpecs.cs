using System;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class HelpTextSpecs : IDisposable
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly FakeInMemoryConsole _console = new();

        public HelpTextSpecs(ITestOutputHelper testOutput) =>
            _testOutput = testOutput;

        public void Dispose()
        {
            _console.DumpToTestOutput(_testOutput);
            _console.Dispose();
        }

        [Fact]
        public async Task Help_text_shows_command_usage_format_which_lists_all_parameters()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<WithParametersCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "--help"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll(
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
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "--help"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll(
                "Usage",
                "cmd", "--opt-a <value>", "--opt-c <values...>", "[options]",
                "Options",
                "* -a|--opt-a",
                "-b|--opt-b",
                "* -c|--opt-c"
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
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"--help"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll(
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
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "--help"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll(
                "Parameters",
                "enum", "Valid values: \"Value1\", \"Value2\", \"Value3\".",
                "Options",
                "--enum", "Valid values: \"Value1\", \"Value2\", \"Value3\".",
                "* --required-enum", "Valid values: \"Value1\", \"Value2\", \"Value3\"."
            );
        }

        [Fact]
        public async Task Help_text_shows_environment_variables_for_options_that_have_them_configured_as_fallback()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<WithEnvironmentVariablesCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "--help"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll(
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
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "--help"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll(
                "Options",
                "--obj", "Default: \"42\"",
                "--str", "Default: \"foo\"",
                "--str-empty", "Default: \"\"",
                "--str-array", "Default: \"foo\" \"bar\" \"baz\"",
                "--bool", "Default: \"True\"",
                "--char", "Default: \"t\"",
                "--int", "Default: \"1337\"",
                "--int-nullable", "Default: \"1337\"",
                "--int-array", "Default: \"1\" \"2\" \"3\"",
                "--timespan", "Default: \"02:03:00\"",
                "--enum", "Default: \"Value2\""
            );
        }
    }
}