﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class HelpTextSpecs : SpecsBase
    {
        public HelpTextSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Help_text_is_printed_if_no_arguments_are_provided_and_the_default_command_is_not_defined()
        {
            // Arrange
            var application = new CliApplicationBuilder()
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
[Command]
public class DefaultCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""default"");
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
        public async Task Help_text_is_printed_if_provided_arguments_contain_the_help_option_even_if_the_default_command_is_not_defined()
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

[Command(""cmd sub"", Description = ""Description of sub command"")]
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
            stdOut.Should().Contain("Description of sub command");
        }

        [Fact]
        public async Task Help_text_is_printed_on_invalid_user_input()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<NoOpCommand>()
                .UseConsole(FakeConsole)
                .SetDescription("This will be in help text")
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
            stdOut.Should().Contain("This will be in help text");
            stdErr.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Help_text_shows_application_metadata()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .UseConsole(FakeConsole)
                .SetTitle("App title")
                .SetDescription("App description")
                .SetVersion("App version")
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll(
                "App title",
                "App description",
                "App version"
            );
        }

        [Fact]
        public async Task Help_text_shows_usage_format_which_lists_all_parameters()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandParameter(0)]
    public string? Foo { get; set; }
    
    [CommandParameter(1)]
    public string? Bar { get; set; }
    
    [CommandParameter(2)]
    public IReadOnlyList<string>? Baz { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
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
            stdOut.Should().ContainAllInOrder(
                "Usage",
                "<foo>", "<bar>", "<baz...>"
            );
        }

        [Fact]
        public async Task Help_text_shows_usage_format_which_lists_all_required_options()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", IsRequired = true)]
    public string? Foo { get; set; }
    
    [CommandOption(""bar"")]
    public string? Bar { get; set; }
    
    [CommandOption(""baz"", IsRequired = true)]
    public IReadOnlyList<string>? Baz { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
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
            stdOut.Should().ContainAllInOrder(
                "Usage",
                "--foo <value>", "--baz <values...>", "[options]",
                "Options",
                "*", "--foo",
                "*", "--baz",
                "--bar"
            );
        }

        [Fact] // TODO: should it?
        public async Task Help_text_shows_usage_format_which_lists_all_available_sub_commands()
        {
            // Arrange
            var commandTypes = DynamicCommandBuilder.CompileMany(
                // language=cs
                @"
[Command]
public class DefaultCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}

[Command(""cmd"")]
public class NamedCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}

[Command(""cmd sub"")]
public class SubCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
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
            stdOut.Should().ContainAllInOrder(
                "Usage",
                "... cmd",
                "... cmd sub"
            );
        }

        [Fact]
        public async Task Help_text_shows_all_available_parameters_and_options()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandParameter(0, Name = ""foo"", Description = ""Description of foo"")]
    public string? Foo { get; set; }
    
    [CommandOption(""bar"", Description = ""Description of bar"")]
    public string? Bar { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
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
            stdOut.Should().ContainAllInOrder(
                "Parameters",
                "foo", "Description of foo",
                "Options",
                "--bar", "Description of bar"
            );
        }

        [Fact]
        public async Task Help_text_shows_the_implicit_help_and_version_options_on_the_default_command()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
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
            stdOut.Should().ContainAllInOrder(
                "Options",
                "-h", "--help", "Shows help text",
                "--version", "Shows version information"
            );
        }

        [Fact]
        public async Task Help_text_shows_the_implicit_help_option_but_not_the_version_option_on_a_named_command()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command(""cmd"")]
public class Command : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
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
            stdOut.Should().ContainAllInOrder(
                "Options",
                "-h", "--help", "Shows help text"
            );
            stdOut.Should().NotContainAny(
                "--version", "Shows version information"
            );
        }

        [Fact]
        public async Task Help_text_shows_all_available_sub_commands()
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

[Command(""cmd one"", Description = ""Description of one command"")]
public class NamedCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""cmd"");
        return default;
    }
}

[Command(""cmd two"", Description = ""Description of another command"")]
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
            stdOut.Should().ContainAllInOrder(
                "Commands",
                "cmd one", "Description of one command",
                "cmd two", "Description of another command"
            );
        }

        [Fact]
        public async Task Help_text_shows_all_valid_values_for_enum_parameters_and_options()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
public enum CustomEnum { One, Two, Three }

[Command]
public class Command : ICommand
{
    [CommandParameter(0)]
    public CustomEnum Foo { get; set; }
    
    [CommandOption(""bar"")]
    public CustomEnum Bar { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
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
            stdOut.Should().ContainAllInOrder(
                "Parameters",
                "foo", "Valid values:", "One", "Two", "Three",
                "Options",
                "--bar", "Valid values:", "One", "Two", "Three"
            );
        }

        [Fact]
        public async Task Help_text_shows_environment_variables_for_options_that_have_them_configured_as_fallback()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
public enum CustomEnum { One, Two, Three }

[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", EnvironmentVariable = ""ENV_FOO"")]
    public CustomEnum Foo { get; set; }
    
    [CommandOption(""bar"", EnvironmentVariable = ""ENV_BAR"")]
    public CustomEnum Bar { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
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
            stdOut.Should().ContainAllInOrder(
                "Options",
                "--foo", "Environment variable:", "ENV_FOO",
                "--bar", "Environment variable:", "ENV_BAR"
            );
        }

        [Fact]
        public async Task Help_text_shows_default_values_for_non_required_options()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
public enum CustomEnum { One, Two, Three }

[Command]
public class Command : ICommand
{
    [CommandOption(""foo"")]
    public object? Foo { get; set; } = 42;

    [CommandOption(""bar"")]
    public string? Bar { get; set; } = ""hello"";

    [CommandOption(""baz"")]
    public IReadOnlyList<string>? Baz { get; set; } = new[] {""one"", ""two"", ""three""};

    [CommandOption(""qwe"")]
    public bool Qwe { get; set; } = true;

    [CommandOption(""qop"")]
    public int? Qop { get; set; } = 1337;

    [CommandOption(""zor"")]
    public TimeSpan Zor { get; set; } = TimeSpan.FromMinutes(123);

    [CommandOption(""lol"")]
    public CustomEnum Lol { get; set; } = CustomEnum.Two;
    
    [CommandOption(""hmm"", IsRequired = true)]
    public string? Hmm { get; set; } = ""not printed"";
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
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
            stdOut.Should().ContainAllInOrder(
                "Options",
                "--foo", "Default:", "42",
                "--bar", "Default:", "hello",
                "--baz", "Default:", "one", "two", "three",
                "--qwe", "Default:", "True",
                "--qop", "Default:", "1337",
                "--zor", "Default:", "02:03:00",
                "--lol", "Default:", "Two"
            );
            stdOut.Should().NotContain("not printed");
        }

        [Fact]
        public async Task Version_text_is_printed_if_provided_arguments_contain_the_version_option()
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