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
            using var console = new BufferedVirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<GenericExceptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput"});

            var stdOut = console.ReadOutputString();
            var stdErr = console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);

            stdOut.Should().BeEmpty();
            stdErr.Should().ContainAll(
                "System.Exception:",
                "Kaput", "at",
                "CliFx.Tests"
            );

            _output.WriteLine(stdOut);
            _output.WriteLine(stdErr);
        }

        [Fact]
        public async Task Command_may_throw_a_generic_exception_with_inner_exception_which_exits_and_prints_error_message_and_stack_trace()
        {
            // Arrange
            var (console, stdOut, stdErr) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<GenericInnerExceptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput", "-i", "FooBar"});

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.GetString().Should().BeEmpty();
            stdErr.GetString().Should().ContainAll(
                "System.Exception:",
                "FooBar",
                "Kaput", "at",
                "CliFx.Tests"
            );

            _output.WriteLine(stdOut.GetString());
            _output.WriteLine(stdErr.GetString());
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_which_exits_with_custom_code_and_prints_minimal_error_details()
        {
            // Arrange
            var (console, stdOut, stdErr) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput", "-c", "69"});

            // Assert
            exitCode.Should().Be(69);
            stdOut.GetString().Should().BeEmpty();
            stdErr.GetString().Trim().Should().Be("Kaput");

            _output.WriteLine(stdOut.GetString());
            _output.WriteLine(stdErr.GetString());
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_without_error_message_which_exits_and_prints_full_error_details()
        {
            // Arrange
            var (console, stdOut, stdErr) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd"});

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.GetString().Should().BeEmpty();
            stdErr.GetString().Should().ContainAll(
                "CliFx.Exceptions.CommandException:",
                "at",
                "CliFx.Tests"
            );

            _output.WriteLine(stdOut.GetString());
            _output.WriteLine(stdErr.GetString());
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_which_exits_and_prints_help_text()
        {
            // Arrange
            var (console, stdOut, stdErr) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput", "--show-help"});

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.GetString().Should().ContainAll(
                "Usage",
                "Options",
                "-h|--help"
            );
            stdErr.GetString().Trim().Should().Be("Kaput");

            _output.WriteLine(stdOut.GetString());
            _output.WriteLine(stdErr.GetString());
        }

        [Fact]
        public async Task Command_shows_help_text_on_invalid_user_input()
        {
            // Arrange
            var (console, stdOut, stdErr) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"not-a-valid-command", "-r", "foo"});

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.GetString().Should().ContainAll(
                "Usage",
                "Options",
                "-h|--help"
            );
            stdErr.GetString().Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdOut.GetString());
            _output.WriteLine(stdErr.GetString());
        }
    }
}