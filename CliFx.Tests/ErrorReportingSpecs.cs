using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class ErrorReportingSpecs : SpecsBase
{
    public ErrorReportingSpecs(ITestOutputHelper testOutput)
        : base(testOutput) { }

    [Fact]
    public async Task I_can_throw_an_exception_in_a_command_to_report_an_error_with_a_stacktrace()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) =>
                    throw new Exception("Something went wrong");
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

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
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) =>
                    throw new Exception("Something went wrong", new Exception("Another exception"));
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

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
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) =>
                    throw new CommandException("Something went wrong", 69);
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

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
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) =>
                    throw new CommandException("", 69);
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(69);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().BeEmpty();

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().ContainAllInOrder("CliFx.Exceptions.CommandException", "at", "CliFx.");
    }

    [Fact]
    public async Task I_can_throw_an_exception_in_a_command_to_report_an_error_and_print_the_help_text()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) =>
                    throw new CommandException("Something went wrong", 69, true);
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .SetDescription("This will be in help text")
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(69);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().Contain("This will be in help text");

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Trim().Should().Be("Something went wrong");
    }
}
