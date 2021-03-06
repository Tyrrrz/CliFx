using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class HelpTextSpecs
    {
        private readonly ITestOutputHelper _output;

        public HelpTextSpecs(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task Help_text_shows_usage_format_which_lists_all_parameters()
        {
            // Arrange
            var (console, stdOut, _) = RedirectedConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<WithParametersCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "--help"});

            // Assert
            exitCode.Should().Be(0);
            stdOut.GetString().Should().ContainAll(
                "Usage",
                "cmd", "<parama>", "<paramb>", "<paramc...>"
            );

            _output.WriteLine(stdOut.GetString());
        }

        [Fact]
        public async Task Help_text_shows_usage_format_which_lists_all_required_options()
        {
            // Arrange
            var (console, stdOut, _) = RedirectedConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<WithRequiredOptionsCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "--help"});

            // Assert
            exitCode.Should().Be(0);
            stdOut.GetString().Should().ContainAll(
                "Usage",
                "cmd", "--opt-a <value>", "--opt-c <values...>", "[options]",
                "Options",
                "* -a|--opt-a",
                "-b|--opt-b",
                "* -c|--opt-c"
            );

            _output.WriteLine(stdOut.GetString());
        }

        [Fact]
        public async Task Help_text_shows_usage_format_which_lists_available_sub_commands()
        {
            // Arrange
            var (console, stdOut, _) = RedirectedConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"--help"});

            // Assert
            exitCode.Should().Be(0);
            stdOut.GetString().Should().ContainAll(
                "Usage",
                "... named",
                "... named sub"
            );

            _output.WriteLine(stdOut.GetString());
        }

        [Fact]
        public async Task Help_text_shows_all_valid_values_for_enum_arguments()
        {
            // Arrange
            var (console, stdOut, _) = RedirectedConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<WithEnumArgumentsCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "--help"});

            // Assert
            exitCode.Should().Be(0);
            stdOut.GetString().Should().ContainAll(
                "Parameters",
                "enum", "Valid values: \"Value1\", \"Value2\", \"Value3\".",
                "Options",
                "--enum", "Valid values: \"Value1\", \"Value2\", \"Value3\".",
                "* --required-enum", "Valid values: \"Value1\", \"Value2\", \"Value3\"."
            );

            _output.WriteLine(stdOut.GetString());
        }

        [Fact]
        public async Task Help_text_shows_environment_variable_names_for_options_that_have_them_defined()
        {
            // Arrange
            var (console, stdOut, _) = RedirectedConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<WithEnvironmentVariablesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "--help"});

            // Assert
            exitCode.Should().Be(0);
            stdOut.GetString().Should().ContainAll(
                "Options",
                "-a|--opt-a", "Environment variable:", "ENV_OPT_A",
                "-b|--opt-b", "Environment variable:", "ENV_OPT_B"
            );

            _output.WriteLine(stdOut.GetString());
        }

        [Fact]
        public async Task Help_text_shows_default_values_for_non_required_options()
        {
            // Arrange
            var (console, stdOut, _) = RedirectedConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<WithDefaultValuesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "--help"});

            // Assert
            exitCode.Should().Be(0);
            stdOut.GetString().Should().ContainAll(
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

            _output.WriteLine(stdOut.GetString());
        }
    }
}