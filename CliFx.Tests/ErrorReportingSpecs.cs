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
            var (console, stdOut, stdErr) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<GenericExceptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput"});

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.GetString().Should().BeEmpty();
            stdErr.GetString().Should().ContainAll(
                typeof(System.Exception).FullName + ":",
                "Kaput", "at",
                typeof(GenericExceptionCommand).FullName + "." + nameof(GenericExceptionCommand.ExecuteAsync)
            );

            _output.WriteLine(stdOut.GetString());
            _output.WriteLine(stdErr.GetString());
        }

        [Fact]
        public async Task Command_may_throw_a_generic_exception_which_exits_and_prints_a_short_error_message_when_told_to()
        {
            // Arrange
            var (console, stdOut, stdErr) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<GenericExceptionCommand>()
                .UseConsole(console)
                .UseShortErrors()
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput"});

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.GetString().Should().BeEmpty();
            stdErr.GetString().Should().ContainAll(
                "Exception:",
                "Kaput", "at",
                nameof(GenericExceptionCommand.ExecuteAsync)
            );
            stdErr.GetString().Should().NotContainAny(
                typeof(System.Exception).FullName + ":",
                typeof(GenericExceptionCommand).FullName
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