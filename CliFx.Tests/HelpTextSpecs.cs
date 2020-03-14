using System.Threading.Tasks;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class HelpTextSpecs
    {
        [Fact]
        public async Task Version_stuff()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(HelloWorldDefaultCommand))
                .AddCommand(typeof(ConcatCommand))
                .AddCommand(typeof(DivideCommand))
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
        public async Task Top_level_help_text_contains_application_metadata()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(HelloWorldDefaultCommand))
                .UseTitle("AppTitle")
                .UseExecutableName("AppExe")
                .UseVersionText("AppVersion")
                .UseDescription("AppDescription")
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"--help"});
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            stdOut.Should().ContainAll("AppTitle", "AppExe", "AppVersion", "AppDescription");
        }

        [Fact]
        public async Task Top_level_help_text_contains_information_about_default_command_if_it_is_defined()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(HelpDefaultCommand))
                .AddCommand(typeof(HelpNamedCommand))
                .AddCommand(typeof(HelpSubCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] {"--help"});
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            stdOut.Should().ContainAll(
                "Usage",
                "[command]", "[options]",
                "Options",
                "-a|--option-a", "OptionA description.",
                "-b|--option-b", "OptionB description.",
                "-h|--help", "Shows help text.",
                "--version", "Shows version information.",
                "Commands",
                "cmd", "HelpNamedCommand description.",
                "You can run", "to show help on a specific command."
            );
        }
    }
}