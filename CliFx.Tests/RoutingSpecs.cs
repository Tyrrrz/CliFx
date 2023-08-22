using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class RoutingSpecs : SpecsBase
{
    public RoutingSpecs(ITestOutputHelper testOutput)
        : base(testOutput) { }

    [Fact]
    public async Task I_can_configure_a_command_to_be_executed_by_default_when_the_user_does_not_specify_a_command_name()
    {
        // Arrange
        var commandTypes = DynamicCommandBuilder.CompileMany(
            // lang=csharp
            """
            [Command]
            public class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("default");
                    return default;
                }
            }

            [Command("cmd")]
            public class NamedCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("cmd");
                    return default;
                }
            }

            [Command("cmd child")]
            public class NamedChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("cmd child");
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommands(commandTypes)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("default");
    }

    [Fact]
    public async Task I_can_configure_a_command_to_be_executed_when_the_user_specifies_its_name()
    {
        // Arrange
        var commandTypes = DynamicCommandBuilder.CompileMany(
            // lang=csharp
            """
            [Command]
            public class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("default");
                    return default;
                }
            }

            [Command("cmd")]
            public class NamedCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("cmd");
                    return default;
                }
            }

            [Command("cmd child")]
            public class NamedChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("cmd child");
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommands(commandTypes)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            new[] { "cmd" },
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("cmd");
    }

    [Fact]
    public async Task I_can_configure_a_nested_command_to_be_executed_when_the_user_specifies_its_name()
    {
        // Arrange
        var commandTypes = DynamicCommandBuilder.CompileMany(
            // lang=csharp
            """
            [Command]
            public class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("default");
                    return default;
                }
            }

            [Command("cmd")]
            public class NamedCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("cmd");
                    return default;
                }
            }

            [Command("cmd child")]
            public class NamedChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("cmd child");
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommands(commandTypes)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            new[] { "cmd", "child" },
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("cmd child");
    }
}
