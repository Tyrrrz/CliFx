using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
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
        public async Task Throwing_an_exception_exits_with_a_detailed_error_message()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new Exception(""Something went wrong"");
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().BeEmpty();
            stdErr.Should().ContainAllInOrder(
                "System.Exception", "Something went wrong",
                "at", "CliFx."
            );
        }

        [Fact]
        public async Task Throwing_an_exception_that_contains_an_inner_exception_exits_with_a_detailed_error_message()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new Exception(""Something went wrong"", new Exception(""Another exception""));
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().BeEmpty();
            stdErr.Should().ContainAllInOrder(
                "System.Exception", "Something went wrong",
                "System.Exception", "Another exception",
                "at", "CliFx."
            );
        }

        [Fact]
        public async Task Throwing_a_command_exception_exits_with_the_provided_code_and_custom_message()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException(""Something went wrong"", 69);
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().Be(69);
            stdOut.Should().BeEmpty();
            stdErr.Trim().Should().Be("Something went wrong");
        }

        [Fact]
        public async Task Throwing_a_command_exception_without_a_custom_message_exits_with_detailed_error_message()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException(69);
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().Be(69);
            stdOut.Should().BeEmpty();
            stdErr.Should().ContainAllInOrder(
                "CliFx.Exceptions.CommandException",
                "at", "CliFx."
            );
        }

        [Fact]
        public async Task Throwing_a_command_exception_may_optionally_print_help_text()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException(""Something went wrong"", 69, true);
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();
            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().Be(69);
            stdOut.Should().ContainAllInOrder(
                "Usage",
                "Options",
                "-h|--help"
            );
            stdErr.Trim().Should().Be("Something went wrong");
        }

        [Fact]
        public async Task Failing_on_invalid_user_input_prints_help_text()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<NoOpCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"invalid-command", "--invalid-option"},
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