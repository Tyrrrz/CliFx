using System;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ErrorReportingSpecs : IDisposable
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly FakeInMemoryConsole _console = new();

        public ErrorReportingSpecs(ITestOutputHelper testOutput) =>
            _testOutput = testOutput;

        public void Dispose()
        {
            _console.DumpToTestOutput(_testOutput);
            _console.Dispose();
        }

        [Fact]
        public async Task Throwing_a_generic_exception_exits_with_a_detailed_error_message()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<GenericExceptionCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput"});

            var stdOut = _console.ReadOutputString();
            var stdErr = _console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().BeEmpty();
            stdErr.Should().ContainAll(
                "System.Exception:",
                "Kaput", "at",
                "CliFx.Tests"
            );
        }

        [Fact]
        public async Task Throwing_a_generic_exception_that_contains_an_inner_exception_exits_with_a_detailed_error_message()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<GenericInnerExceptionCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput", "-i", "FooBar"});

            var stdOut = _console.ReadOutputString();
            var stdErr = _console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().BeEmpty();
            stdErr.Should().ContainAll(
                "System.Exception:",
                "FooBar",
                "Kaput", "at",
                "CliFx.Tests"
            );
        }

        [Fact]
        public async Task Throwing_a_specialized_exception_exits_with_the_provided_code_and_custom_message()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput", "-c", "69"});

            var stdOut = _console.ReadOutputString();
            var stdErr = _console.ReadErrorString();

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
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd"});

            var stdOut = _console.ReadOutputString();
            var stdErr = _console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().BeEmpty();
            stdErr.Should().ContainAll(
                "CliFx.Exceptions.CommandException:",
                "at",
                "CliFx.Tests"
            );
        }

        [Fact]
        public async Task Throwing_a_specialized_exception_may_optionally_print_help_text()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"cmd", "-m", "Kaput", "--show-help"});

            var stdOut = _console.ReadOutputString();
            var stdErr = _console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().ContainAll(
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
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"not-a-valid-command", "-r", "foo"});

            var stdOut = _console.ReadOutputString();
            var stdErr = _console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().ContainAll(
                "Usage",
                "Options",
                "-h|--help"
            );
            stdErr.Should().NotBeNullOrWhiteSpace();
        }
    }
}