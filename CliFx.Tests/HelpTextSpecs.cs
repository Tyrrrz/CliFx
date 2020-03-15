using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class HelpTextSpecs
    {
        [Fact]
        public async Task Version_information_can_be_requested_by_providing_the_version_option_without_other_arguments()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(DefaultCommand))
                .AddCommand(typeof(NamedCommand))
                .AddCommand(typeof(NamedSubCommand))
                .UseVersionText("v6.9")
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"--version"});
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Be("v6.9");
        }

        [Fact]
        public async Task Help_text_can_be_requested_by_providing_the_help_option()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(DefaultCommand))
                .AddCommand(typeof(NamedCommand))
                .AddCommand(typeof(NamedSubCommand))
                .UseTitle("AppTitle")
                .UseVersionText("AppVer")
                .UseDescription("AppDesc")
                .UseExecutableName("AppExe")
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"--help"});
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            stdOut.Should().ContainAll(
                "AppTitle", "AppVer",
                "AppDesc",
                "Usage",
                "AppExe", "[command]", "[options]",
                "Options",
                "-a|--option-a", "OptionA description.",
                "-b|--option-b", "OptionB description.",
                "-h|--help", "Shows help text.",
                "--version", "Shows version information.",
                "Commands",
                "cmd", "NamedCommand description.",
                "You can run", "to show help on a specific command."
            );
        }

        [Fact]
        public async Task Help_text_can_be_requested_on_a_specific_named_command()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(DefaultCommand))
                .AddCommand(typeof(NamedCommand))
                .AddCommand(typeof(NamedSubCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"cmd", "--help"});
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            stdOut.Should().ContainAll(
                "Description",
                "NamedCommand description.",
                "Usage",
                "cmd", "[command]", "<param-a>", "[options]",
                "Parameters",
                "* param-a", "ParameterA description.",
                "Options",
                "-c|--option-c", "OptionC description.",
                "-d|--option-d", "OptionD description.",
                "-h|--help", "Shows help text.",
                "Commands",
                "sub", "SubCommand description.",
                "You can run", "to show help on a specific command."
            );
        }

        [Fact]
        public async Task Help_text_can_be_requested_on_a_specific_named_sub_command()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(DefaultCommand))
                .AddCommand(typeof(NamedCommand))
                .AddCommand(typeof(NamedSubCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"cmd", "sub", "--help"});
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            stdOut.Should().ContainAll(
                "Description",
                "SubCommand description.",
                "Usage",
                "cmd sub", "<param-b>", "<param-c>", "[options]",
                "Parameters",
                "* param-b", "ParameterB description.",
                "* param-c", "ParameterC description.",
                "Options",
                "-e|--option-e", "OptionE description.",
                "-h|--help", "Shows help text."
            );
        }

        [Fact]
        public async Task Help_text_can_be_requested_without_specifying_command_even_if_default_command_is_not_defined()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(NamedCommand))
                .AddCommand(typeof(NamedSubCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"--help"});
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            stdOut.Should().ContainAll(
                "Usage",
                "[command]",
                "Options",
                "-h|--help", "Shows help text.",
                "--version", "Shows version information.",
                "Commands",
                "cmd", "NamedCommand description.",
                "You can run", "to show help on a specific command."
            );
        }

        [Fact]
        public async Task Help_text_shows_usage_format_which_lists_all_parameters()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(ParametersCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"cmd-with-params", "--help"});
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            stdOut.Should().ContainAll(
                "Usage",
                "cmd-with-params", "<first>", "<parameterb>", "<third list...>", "[options]"
            );
        }

        [Fact]
        public async Task Help_text_shows_usage_format_which_lists_all_required_options()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(RequiredOptionsCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"cmd-with-req-opts", "--help"});
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            stdOut.Should().ContainAll(
                "Usage",
                "cmd-with-req-opts", "--option-f <value>", "--option-g <values...>", "[options]",
                "Options",
                "* -f|--option-f",
                "* -g|--option-g",
                "-h|--option-h"
            );
        }

        [Fact]
        public async Task Help_text_lists_environment_variable_names_for_options_that_have_them_defined()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(EnvironmentVariableCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"cmd-with-env-vars", "--help"});
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            stdOut.Should().ContainAll(
                "Options",
                "* -a|--option-a", "Environment variable:", "ENV_OPT_A",
                "-b|--option-b", "Environment variable:", "ENV_OPT_B"
            );
        }
    }
}