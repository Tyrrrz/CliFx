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
        public async Task Command_may_throw_a_generic_exception_which_exits_and_prints_full_error_details()
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
            stdErrData.Should().Contain("Kaput");
            stdErrData.Length.Should().BeGreaterThan("Kaput".Length);
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
    }
}