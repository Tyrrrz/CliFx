using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class RoutingSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public async Task I_can_execute_the_default_command()
    {
        // Arrange
        var commands = CommandCompiler.CompileMany(
            // lang=csharp
            """
            [Command]
            public partial class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("default");
                    return default;
                }
            }

            [Command("cmd")]
            public partial class NamedCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("cmd");
                    return default;
                }
            }

            [Command("cmd child")]
            public partial class NamedChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("cmd child");
                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommands(commands)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync([], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("default");
    }

    [Fact]
    public async Task I_can_execute_a_named_command()
    {
        // Arrange
        var commands = CommandCompiler.CompileMany(
            // lang=csharp
            """
            [Command]
            public partial class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("default");
                    return default;
                }
            }

            [Command("cmd")]
            public partial class NamedCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("cmd");
                    return default;
                }
            }

            [Command("cmd child")]
            public partial class NamedChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("cmd child");
                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommands(commands)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["cmd"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("cmd");
    }

    [Fact]
    public async Task I_can_execute_a_nested_named_command()
    {
        // Arrange
        var commands = CommandCompiler.CompileMany(
            // lang=csharp
            """
            [Command]
            public partial class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("default");
                    return default;
                }
            }

            [Command("cmd")]
            public partial class NamedCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("cmd");
                    return default;
                }
            }

            [Command("cmd child")]
            public partial class NamedChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("cmd child");
                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommands(commands)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["cmd", "child"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("cmd child");
    }
}
