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

[Command(""cmd sub"")]
public class SubCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd sub"");
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

[Command(""cmd sub"")]
public class SubCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd sub"");
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
        public async Task Specific_named_sub_command_is_executed_if_provided_arguments_match_its_name()
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

[Command(""cmd sub"")]
public class SubCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd sub"");
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
                new[] {"cmd", "sub"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("cmd sub");
        }

        [Fact]
        public async Task Help_text_is_printed_if_no_arguments_were_provided_and_default_command_is_not_defined()
        {
            // Arrange
            var commandTypes = DynamicCommandBuilder.CompileMany(
                // language=cs
                @"
[Command(""cmd"")]
public class NamedCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd"");
        return default;
    }
}

[Command(""cmd sub"")]
public class SubCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd sub"");
        return default;
    }
}
");

            var application = new CliApplicationBuilder()
                .AddCommands(commandTypes)
                .UseConsole(FakeConsole)
                .SetDescription("This will be in help text")
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Contain("This will be in help text");
        }

        [Fact]
        public async Task Help_text_is_printed_if_provided_arguments_contain_the_help_option()
        {
            // Arrange
            var commandTypes = DynamicCommandBuilder.CompileMany(
                // language=cs
                @"
[Command(Description = ""Description of default command"")]
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

[Command(""cmd sub"")]
public class SubCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd sub"");
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
                new[] {"--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Contain("Description of default command");
        }

        [Fact]
        public async Task Help_text_is_printed_if_provided_arguments_contain_the_help_option_even_if_default_command_is_not_defined()
        {
            // Arrange
            var commandTypes = DynamicCommandBuilder.CompileMany(
                // language=cs
                @"
[Command(""cmd"")]
public class NamedCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd"");
        return default;
    }
}

[Command(""cmd sub"")]
public class SubCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd sub"");
        return default;
    }
}
");

            var application = new CliApplicationBuilder()
                .AddCommands(commandTypes)
                .UseConsole(FakeConsole)
                .SetDescription("This will be in help text")
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Contain("This will be in help text");
        }

        [Fact]
        public async Task Help_text_for_a_specific_named_command_is_printed_if_provided_arguments_match_its_name_and_contain_the_help_option()
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

[Command(""cmd"", Description = ""Description of named command"")]
public class NamedCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd"");
        return default;
    }
}

[Command(""cmd sub"")]
public class SubCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd sub"");
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
                new[] {"cmd", "--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Contain("Description of named command");
        }

        [Fact]
        public async Task Help_text_for_a_specific_named_sub_command_is_printed_if_provided_arguments_match_its_name_and_contain_the_help_option()
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

[Command(""cmd sub"", Description = ""Description of named command"")]
public class SubCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd sub"");
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
                new[] {"cmd", "sub", "--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Contain("Description of named command");
        }

        [Fact]
        public async Task Version_text_is_printed_if_the_only_provided_argument_is_the_version_option()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<NoOpCommand>()
                .SetVersion("v6.9")
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--version"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("v6.9");
        }
    }
}