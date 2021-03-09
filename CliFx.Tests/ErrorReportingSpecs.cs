using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ErrorReportingSpecs : SpecsBase
    {
        public ErrorReportingSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Throwing_a_generic_exception_exits_with_a_detailed_error_message()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<GenericExceptionCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "-m", "Kaput"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().BeEmpty();
            stdErr.Should().ContainAllInOrder(
                "System.Exception:", "Kaput",
                "at", "CliFx.Tests"
            );
        }

        [Fact]
        public async Task Throwing_a_generic_exception_that_contains_an_inner_exception_exits_with_a_detailed_error_message()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<GenericInnerExceptionCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "-m", "Kaput", "-i", "FooBar"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().BeEmpty();
            stdErr.Should().ContainAllInOrder(
                "System.Exception:", "Kaput",
                "System.Exception:", "FooBar",
                "at", "CliFx.Tests"
            );
        }

        [Fact]
        public async Task Throwing_a_specialized_exception_exits_with_the_provided_code_and_custom_message()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "-m", "Kaput", "-c", "69"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().Be(69);
            stdOut.Should().BeEmpty();
            stdErr.Trim().Should().Be("Kaput");
        }

        [Fact]
        public async Task Throwing_a_specialized_exception_without_a_custom_message_exits_with_detailed_error_message()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().BeEmpty();
            stdErr.Should().ContainAllInOrder(
                "CliFx.Exceptions.CommandException:",
                "at", "CliFx.Tests"
            );
        }

        [Fact]
        public async Task Throwing_a_specialized_exception_may_optionally_print_help_text()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "-m", "Kaput", "--show-help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().ContainAllInOrder(
                "Usage",
                "Options",
                "-h|--help"
            );
            stdErr.Trim().Should().Be("Kaput");
        }

        [Fact]
        public async Task Failing_on_invalid_user_input_prints_help_text()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"not-a-valid-command", "-r", "foo"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().ContainAllInOrder(
                "Usage",
                "Options",
                "-h|--help"
            );
            stdErr.Should().NotBeNullOrWhiteSpace();
        }
    }
}