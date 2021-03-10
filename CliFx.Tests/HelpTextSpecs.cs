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
        public async Task Help_text_shows_command_usage_format_which_lists_all_parameters()
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
        public async Task Help_text_shows_command_usage_format_which_lists_all_required_options()
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

        [Fact]
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
        public async Task Help_text_shows_all_valid_values_for_enum_arguments()
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
                new[] {"cmd", "--help"},
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
    public IReadOnlyList<string>? Baz { get; set; } = new []{""one"", ""two"", ""three""};

    [CommandOption(""qwe"")]
    public bool Qwe { get; set; } = true;

    [CommandOption(""qop"")]
    public int? Qop { get; set; } = 1337;

    [CommandOption(""zor"")]
    public TimeSpan Zor { get; set; } = TimeSpan.FromMinutes(123);

    [CommandOption(""lol"")]
    public CustomEnum Lol { get; set; } = CustomEnum.Two;
    
    [CommandOption(""hm"", IsRequired = true)]
    public string? Hm { get; set; } = ""not printed"";
    
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
    }
}