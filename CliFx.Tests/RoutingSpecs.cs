using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class RoutingSpecs : SpecsBase
    {
        public RoutingSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Default_command_is_executed_if_provided_arguments_do_not_match_any_named_command()
        {
            // Arrange
            var commandTypes = DynamicCommandBuilder.CompileMany(
                // language=cs
                @"
[Command]
public class DefaultCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""default"");
        return default;
    }
}

[Command(""cmd"")]
public class NamedCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd"");
        return default;
    }
}

[Command(""cmd child"")]
public class NamedChildCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd child"");
        return default;
    }
}
");

            var application = new CliApplicationBuilder()
                .AddCommands(commandTypes)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("default");
        }

        [Fact]
        public async Task Specific_named_command_is_executed_if_provided_arguments_match_its_name()
        {
            // Arrange
            var commandTypes = DynamicCommandBuilder.CompileMany(
                // language=cs
                @"
[Command]
public class DefaultCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""default"");
        return default;
    }
}

[Command(""cmd"")]
public class NamedCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd"");
        return default;
    }
}

[Command(""cmd child"")]
public class NamedChildCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd child"");
        return default;
    }
}
");

            var application = new CliApplicationBuilder()
                .AddCommands(commandTypes)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("cmd");
        }

        [Fact]
        public async Task Specific_named_child_command_is_executed_if_provided_arguments_match_its_name()
        {
            // Arrange
            var commandTypes = DynamicCommandBuilder.CompileMany(
                // language=cs
                @"
[Command]
public class DefaultCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""default"");
        return default;
    }
}

[Command(""cmd"")]
public class NamedCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd"");
        return default;
    }
}

[Command(""cmd child"")]
public class NamedChildCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd child"");
        return default;
    }
}
");

            var application = new CliApplicationBuilder()
                .AddCommands(commandTypes)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "child"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("cmd child");
        }
    }
}