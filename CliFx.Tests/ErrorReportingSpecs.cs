using System.IO;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ErrorReportingSpecs
    {
        private readonly ITestOutputHelper _output;

        public ErrorReportingSpecs(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task Command_may_throw_a_generic_exception_which_exits_and_prints_error_message_and_stack_trace()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            await using var stdErr = new MemoryStream();

            var console = new VirtualConsole(output: stdOut, error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<GenericExceptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput"});

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdOutData.Should().BeEmpty();
            stdErrData.Should().ContainAll(
                "System.Exception:",
                "Kaput", "at",
                "CliFx.Tests"
            );

            _output.WriteLine(stdOutData);
            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_which_exits_with_custom_code_and_prints_minimal_error_details()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            await using var stdErr = new MemoryStream();

            var console = new VirtualConsole(output: stdOut, error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput", "-c", "69"});

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().Be(69);
            stdOutData.Should().BeEmpty();
            stdErrData.Should().Be("Kaput");

            _output.WriteLine(stdOutData);
            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_without_error_message_which_exits_and_prints_full_error_details()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            await using var stdErr = new MemoryStream();

            var console = new VirtualConsole(output: stdOut, error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd"});

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdOutData.Should().BeEmpty();
            stdErrData.Should().ContainAll(
                "CliFx.Exceptions.CommandException:",
                "at",
                "CliFx.Tests"
            );

            _output.WriteLine(stdOutData);
            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_which_exits_and_prints_help_text()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            await using var stdErr = new MemoryStream();

            var console = new VirtualConsole(output: stdOut, error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput", "--show-help"});

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdOutData.Should().ContainAll(
                "Usage",
                "Options",
                "-h|--help"
            );
            stdErrData.Should().Be("Kaput");

            _output.WriteLine(stdOutData);
            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_shows_help_text_on_invalid_user_input()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            await using var stdErr = new MemoryStream();

            var console = new VirtualConsole(output: stdOut, error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"not-a-valid-command", "-r", "foo"});

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdOutData.Should().ContainAll(
                "Usage",
                "Options",
                "-h|--help"
            );
            stdErrData.Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdOutData);
            _output.WriteLine(stdErrData);
        }
    }
}