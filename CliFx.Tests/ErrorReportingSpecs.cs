using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class ErrorReportingSpecs
    {
        [Fact]
        public async Task If_the_executed_command_throws_a_generic_exception_then_the_message_and_stack_trace_is_printed()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(GenericExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"exc", "-m", "Kaput"}, new Dictionary<string, string>());
            var stdErr = console.ReadErrorString().TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("Kaput");
            stdErr.Length.Should().BeGreaterThan("Kaput".Length);
        }

        [Fact]
        public async Task If_the_executed_command_throws_a_command_exception_then_only_the_message_is_printed_and_specified_exit_code_is_returned()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CommandExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"exc", "-m", "Kaput", "-c", "69"}, new Dictionary<string, string>());
            var stdErr = console.ReadErrorString().TrimEnd();

            // Assert
            exitCode.Should().Be(69);
            stdErr.Should().Be("Kaput");
        }

        [Fact]
        public async Task If_the_executed_command_throws_a_command_exception_that_does_not_have_a_message_then_the_stack_trace_is_printed()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CommandExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"exc", "-m", "Kaput"}, new Dictionary<string, string>());
            var stdErr = console.ReadErrorString().TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().NotBeEmpty();
        }
    }
}