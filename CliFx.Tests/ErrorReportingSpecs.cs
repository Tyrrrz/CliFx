using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class ErrorReportingSpecs
    {
        [Fact]
        public async Task Command_may_throw_a_generic_exception_which_exits_and_prints_error_message_and_stack_trace()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(GenericExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"exc", "-m", "Kaput"},
                new Dictionary<string, string>());

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().ContainAll(
                "System.Exception:",
                "Kaput", "at", 
                "CliFx.Tests");
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_which_exits_with_custom_code_and_prints_minimal_error_details()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CommandExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"exc", "-m", "Kaput", "-c", "69"},
                new Dictionary<string, string>());

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().Be(69);
            stdErrData.Should().Be("Kaput");
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_without_error_message_which_exits_and_prints_full_error_details()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CommandExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"exc", "-m", "Kaput"},
                new Dictionary<string, string>());

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_which_shows_only_the_help_text()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            await using var stdErr = new MemoryStream();

            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(ShowHelpTextOnlyCommand))
                .AddCommand(typeof(ShowHelpTextOnlySubCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] { "exc" });
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();
            var stdErrData = console.Output.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            stdErrData.Should().BeEmpty();
            stdOutData.Should().ContainAll(
                "Usage",
                "[command]",
                "Options",
                "-h|--help", "Shows help text.",
                "Commands",
                "sub",
                "You can run", "to show help on a specific command."
            );
        }

        [Fact]
        public async Task Command_may_throw_specialized_exception_which_shows_the_error_message_then_the_help_text()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(output: stdOut, error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(ShowErrorMessageThenHelpTextCommand))
                .AddCommand(typeof(ShowErrorMessageThenHelpTextSubCommand))
                .UseConsole(console)
                .Build();

            // Act
            await application.RunAsync(new[] { "exc" });
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            stdErrData.Should().Be("Error message.");
            stdOutData.Should().ContainAll(
                "Usage",
                "[command]",
                "Options",
                "-h|--help", "Shows help text.",
                "Commands",
                "sub",
                "You can run", "to show help on a specific command."
            );
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_which_shows_only_a_stack_trace_and_no_help_text()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(GenericExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] { "exc", "-m", "Kaput" },
                new Dictionary<string, string>());

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().Contain("Kaput");
            stdErrData.Length.Should().BeGreaterThan("Kaput".Length);
        }
    }
}