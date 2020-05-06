using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public partial class HelpTextSpecs
    {
        private readonly ITestOutputHelper _output;

        public HelpTextSpecs(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task Version_information_can_be_requested_by_providing_the_version_option_without_other_arguments()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(DefaultCommand))
                .AddCommand(typeof(NamedCommand))
                .AddCommand(typeof(NamedSubCommand))
                .UseVersionText("v6.9")
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"--version"});
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOutData.Should().Be("v6.9");

            _output.WriteLine(stdOutData);
        }

        [Fact]
        public async Task Help_text_can_be_requested_by_providing_the_help_option()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

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
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            stdOutData.Should().ContainAll(
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

            _output.WriteLine(stdOutData);
        }

        [Fact]
        public async Task Help_text_can_be_requested_on_a_specific_named_command()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(DefaultCommand))
                .AddCommand(typeof(NamedCommand))
                .AddCommand(typeof(NamedSubCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"cmd", "--help"});
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            stdOutData.Should().ContainAll(
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

            _output.WriteLine(stdOutData);
        }

        [Fact]
        public async Task Help_text_can_be_requested_on_a_specific_named_sub_command()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(DefaultCommand))
                .AddCommand(typeof(NamedCommand))
                .AddCommand(typeof(NamedSubCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"cmd", "sub", "--help"});
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            stdOutData.Should().ContainAll(
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

            _output.WriteLine(stdOutData);
        }

        [Fact]
        public async Task Help_text_can_be_requested_without_specifying_command_even_if_default_command_is_not_defined()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(NamedCommand))
                .AddCommand(typeof(NamedSubCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"--help"});
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            stdOutData.Should().ContainAll(
                "Usage",
                "[command]",
                "Options",
                "-h|--help", "Shows help text.",
                "--version", "Shows version information.",
                "Commands",
                "cmd", "NamedCommand description.",
                "You can run", "to show help on a specific command."
            );

            _output.WriteLine(stdOutData);
        }

        [Fact]
        public async Task Help_text_shows_usage_format_which_lists_all_parameters()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(ParametersCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"cmd-with-params", "--help"});
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            stdOutData.Should().ContainAll(
                "Usage",
                "cmd-with-params", "<first>", "<parameterb>", "<third list...>", "[options]"
            );

            _output.WriteLine(stdOutData);
        }

        [Fact]
        public async Task Help_text_shows_usage_format_which_lists_all_required_options()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(RequiredOptionsCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"cmd-with-req-opts", "--help"});
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            stdOutData.Should().ContainAll(
                "Usage",
                "cmd-with-req-opts", "--option-f <value>", "--option-g <values...>", "[options]",
                "Options",
                "* -f|--option-f",
                "* -g|--option-g",
                "-h|--option-h"
            );

            _output.WriteLine(stdOutData);
        }

        [Fact]
        public async Task Help_text_shows_usage_format_which_lists_all_valid_values_for_enum_options()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(EnumOptionsCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] { "cmd-with-enum-opts", "--help" });
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            stdOutData.Should().ContainAll(
                "Usage",
                "cmd-with-enum-opts", "[options]",
                "Options",
                "* --value", "Enum option.", "Valid values: value1, value2, value3.",
                "--empty"
            );
            // Make sure we're not rendering anything for empty enums.
            stdOutData.Should().NotContain("--empty           Valid values:");

            _output.WriteLine(stdOutData);
        }

        [Fact]
        public async Task Help_text_lists_environment_variable_names_for_options_that_have_them_defined()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(EnvironmentVariableCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"cmd-with-env-vars", "--help"});
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            stdOutData.Should().ContainAll(
                "Options",
                "* -a|--option-a", "Environment variable:", "ENV_OPT_A",
                "-b|--option-b", "Environment variable:", "ENV_OPT_B"
            );

            _output.WriteLine(stdOutData);
        }
    }
}