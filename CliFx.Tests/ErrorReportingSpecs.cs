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
        : base(testOutput)
    {
    }

    [Fact]
    public async Task Command_can_throw_an_exception_which_exits_with_a_stacktrace()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
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
    public async Task Command_can_throw_an_exception_with_an_inner_exception_which_exits_with_a_stacktrace()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
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
    public async Task Command_can_throw_a_special_exception_which_exits_with_specified_code_and_message()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
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

        var stdOut = FakeConsole.ReadOutputString();
        var stdErr = FakeConsole.ReadErrorString();

        // Assert
        exitCode.Should().Be(69);
        stdOut.Should().BeEmpty();
        stdErr.Trim().Should().Be("Something went wrong");
    }

    [Fact]
    public async Task Command_can_throw_a_special_exception_without_message_which_exits_with_a_stacktrace()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
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
    public async Task Command_can_throw_a_special_exception_which_prints_help_text_before_exiting()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
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

        var stdOut = FakeConsole.ReadOutputString();
        var stdErr = FakeConsole.ReadErrorString();

        // Assert
        exitCode.Should().Be(69);
        stdOut.Should().Contain("This will be in help text");
        stdErr.Trim().Should().Be("Something went wrong");
    }
}