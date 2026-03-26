using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class ErrorReportingSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public async Task I_can_throw_an_exception_in_a_command_to_report_an_error_with_a_stacktrace()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) =>
                    throw new Exception("Something went wrong");
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync([], new Dictionary<string, string>());

        // Assert
        exitCode.Should().NotBe(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().BeEmpty();

        var stdErr = FakeConsole.ReadErrorString();
        stdErr
            .Should()
            .ContainAllInOrder("System.Exception", "Something went wrong", "at", "CliFx.");
    }

    [Fact]
    public async Task I_can_throw_an_exception_with_an_inner_exception_in_a_command_to_report_an_error_with_a_stacktrace()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) =>
                    throw new Exception("Something went wrong", new Exception("Another exception"));
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync([], new Dictionary<string, string>());

        // Assert
        exitCode.Should().NotBe(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().BeEmpty();

        var stdErr = FakeConsole.ReadErrorString();
        stdErr
            .Should()
            .ContainAllInOrder(
                "System.Exception",
                "Something went wrong",
                "System.Exception",
                "Another exception",
                "at",
                "CliFx."
            );
    }

    [Fact]
    public async Task I_can_throw_an_exception_in_a_command_to_report_an_error_and_exit_with_the_specified_code()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) =>
                    throw new CommandException("Something went wrong", 69);
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync([], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(69);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().BeEmpty();

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Trim().Should().Be("Something went wrong");
    }

    [Fact]
    public async Task I_can_throw_an_exception_without_a_message_in_a_command_to_report_an_error_with_a_stacktrace()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) =>
                    throw new CommandException("", 69);
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync([], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(69);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().BeEmpty();

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().ContainAllInOrder("CliFx.CommandException", "at", "CliFx.");
    }

    [Fact]
    public async Task I_can_throw_an_exception_in_a_command_to_report_an_error_and_show_help()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) =>
                    throw new CommandException("Something went wrong", 69, true);
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .SetDescription("This will be in the help text")
            .Build();

        // Act
        var exitCode = await application.RunAsync([], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(69);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().Contain("This will be in the help text");

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Trim().Should().Be("Something went wrong");
    }
}
