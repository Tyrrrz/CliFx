using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class HelpTextSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public async Task I_can_request_the_help_text_by_running_the_application_without_arguments_if_the_default_command_is_not_defined()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .UseConsole(FakeConsole)
            .SetDescription("This will be in help text")
            .Build();

        // Act
        var exitCode = await application.RunAsync([], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(1);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().Contain("This will be in help text");
    }

    [Fact]
    public async Task I_can_request_the_help_text_by_running_the_application_with_the_implicit_help_option()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .SetDescription("This will be in help text")
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().Contain("This will be in help text");
    }

    [Fact]
    public async Task I_can_request_the_help_text_by_running_the_application_with_the_implicit_help_option_even_if_the_default_command_is_not_defined()
    {
        // Arrange
        var command = CommandCompiler.CompileMany(
            // lang=csharp
            """
            [Command("cmd")]
            public partial class NamedCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd child")]
            public partial class NamedChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommands(command)
            .UseConsole(FakeConsole)
            .SetDescription("This will be in help text")
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().Contain("This will be in help text");
    }

    [Fact]
    public async Task I_can_request_the_help_text_for_a_specific_command_by_running_the_application_and_specifying_its_name_with_the_implicit_help_option()
    {
        // Arrange
        var command = CommandCompiler.CompileMany(
            // lang=csharp
            """
            [Command]
            public partial class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd", Description = "Description of a named command.")]
            public partial class NamedCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd child")]
            public partial class NamedChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommands(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["cmd", "--help"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().Contain("Description of a named command.");
    }

    [Fact]
    public async Task I_can_request_the_help_text_for_a_specific_nested_command_by_running_the_application_and_specifying_its_name_with_the_implicit_help_option()
    {
        // Arrange
        var command = CommandCompiler.CompileMany(
            // lang=csharp
            """
            [Command]
            public partial class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd")]
            public partial class NamedCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd child", Description = "Description of a named child command.")]
            public partial class NamedChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommands(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["cmd", "sub", "--help"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().Contain("Description of a named child command.");
    }

    [Fact]
    public async Task I_can_request_the_help_text_by_running_the_application_with_invalid_arguments()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommand(NoOpCommand.Descriptor)
            .UseConsole(FakeConsole)
            .SetDescription("This will be in help text")
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["invalid-command", "--invalid-option"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().NotBe(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().Contain("This will be in help text");

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_application_title_description_and_version()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .UseConsole(FakeConsole)
            .SetTitle("App title")
            .SetDescription("App description")
            .SetVersion("App version")
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ContainAll("App title", "App description", "App version");
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_command_description()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command(Description = "Description of the default command.")]
            public partial class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ContainAllInOrder("DESCRIPTION", "Description of the default command.");
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_usage_format_for_a_named_command()
    {
        // Arrange
        var command = CommandCompiler.CompileMany(
            // lang=csharp
            """
            [Command]
            public partial class DefaultCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd")]
            public partial class NamedCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommands(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ContainAllInOrder("USAGE", "[command]", "[...]");
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_usage_format_for_all_parameters()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandParameter(0)]
                public required string Foo { get; set; }

                [CommandParameter(1)]
                public required string Bar { get; set; }

                [CommandParameter(2)]
                public required IReadOnlyList<string> Baz { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ContainAllInOrder("USAGE", "<foo>", "<bar>", "<baz...>");
    }

    // https://github.com/Tyrrrz/CliFx/issues/117
    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_usage_format_for_all_parameters_in_the_correct_order()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            // Base members appear last in reflection order
            public abstract class CommandBase : ICommand
            {
                [CommandParameter(0)]
                public required string Foo { get; set; }

                public abstract ValueTask ExecuteAsync(IConsole console);
            }

            [Command]
            public partial class Command : CommandBase
            {
                [CommandParameter(2)]
                public required IReadOnlyList<string> Baz { get; set; }

                [CommandParameter(1)]
                public required string Bar { get; set; }

                public override ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ContainAllInOrder("USAGE", "<foo>", "<bar>", "<baz...>");
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_usage_format_for_all_required_options()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo")]
                public required string Foo { get; set; }

                [CommandOption("bar")]
                public string? Bar { get; set; }

                [CommandOption("baz")]
                public required IReadOnlyList<string> Baz { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut
            .Should()
            .ContainAllInOrder("USAGE", "--foo <value>", "--baz <values...>", "[options]");
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_list_of_all_parameters_and_options()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandParameter(0, Name = "foo", Description = "Description of foo.")]
                public string? Foo { get; set; }

                [CommandOption("bar", Description = "Description of bar.")]
                public string? Bar { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut
            .Should()
            .ContainAllInOrder(
                "PARAMETERS",
                "foo",
                "Description of foo.",
                "OPTIONS",
                "--bar",
                "Description of bar."
            );
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_help_and_implicit_version_options()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut
            .Should()
            .ContainAllInOrder(
                "OPTIONS",
                "-h",
                "--help",
                "Shows help text",
                "--version",
                "Shows version information"
            );
    }

    [Fact]
    public async Task I_can_request_the_help_text_on_a_named_command_to_see_the_implicit_help_option()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command("cmd")]
            public partial class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["cmd", "--help"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();

        stdOut.Should().ContainAllInOrder("OPTIONS", "-h", "--help", "Shows help text");

        stdOut.Should().NotContainAny("--version", "Shows version information");
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_list_of_valid_values_for_all_parameters_and_options_bound_to_enum_properties()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            public enum CustomEnum { One, Two, Three }

            [Command]
            public partial class Command : ICommand
            {
                [CommandParameter(0)]
                public CustomEnum Foo { get; set; }

                [CommandOption("bar")]
                public CustomEnum Bar { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut
            .Should()
            .ContainAllInOrder(
                "PARAMETERS",
                "foo",
                "Choices:",
                "One",
                "Two",
                "Three",
                "OPTIONS",
                "--bar",
                "Choices:",
                "One",
                "Two",
                "Three"
            );
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_list_of_valid_values_for_all_parameters_and_options_bound_to_nullable_enum_properties()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            public enum CustomEnum { One, Two, Three }

            [Command]
            public partial class Command : ICommand
            {
                [CommandParameter(0)]
                public CustomEnum? Foo { get; set; }

                [CommandOption("bar")]
                public CustomEnum? Bar { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut
            .Should()
            .ContainAllInOrder(
                "PARAMETERS",
                "foo",
                "Choices:",
                "One",
                "Two",
                "Three",
                "OPTIONS",
                "--bar",
                "Choices:",
                "One",
                "Two",
                "Three"
            );
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_environment_variables_of_options_that_use_them_as_fallback()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            public enum CustomEnum { One, Two, Three }

            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo", EnvironmentVariable = "ENV_FOO")]
                public CustomEnum Foo { get; set; }

                [CommandOption("bar", EnvironmentVariable = "ENV_BAR")]
                public CustomEnum Bar { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut
            .Should()
            .ContainAllInOrder(
                "OPTIONS",
                "--foo",
                "Environment variable:",
                "ENV_FOO",
                "--bar",
                "Environment variable:",
                "ENV_BAR"
            );
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_default_values_of_non_required_options()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            public enum CustomEnum { One, Two, Three }

            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo")]
                public object? Foo { get; set; } = 42;

                [CommandOption("bar")]
                public string? Bar { get; set; } = "hello";

                [CommandOption("baz")]
                public IReadOnlyList<string>? Baz { get; set; } = new[] {"one", "two", "three"};

                [CommandOption("qwe")]
                public bool Qwe { get; set; } = true;

                [CommandOption("qop")]
                public int? Qop { get; set; } = 1337;

                [CommandOption("zor")]
                public TimeSpan Zor { get; set; } = TimeSpan.FromMinutes(123);

                [CommandOption("lol")]
                public CustomEnum Lol { get; set; } = CustomEnum.Two;

                [CommandOption("hmm")]
                public required string Hmm { get; set; } = "not printed";

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();

        stdOut
            .Should()
            .ContainAllInOrder(
                "OPTIONS",
                "--foo",
                "Default:",
                "42",
                "--bar",
                "Default:",
                "hello",
                "--baz",
                "Default:",
                "one",
                "two",
                "three",
                "--qwe",
                "Default:",
                "True",
                "--qop",
                "Default:",
                "1337",
                "--zor",
                "Default:",
                "02:03:00",
                "--lol",
                "Default:",
                "Two"
            );

        stdOut.Should().NotContain("not printed");
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_list_of_all_immediate_child_commands()
    {
        // Arrange
        var command = CommandCompiler.CompileMany(
            // lang=csharp
            """
            [Command("cmd1", Description = "Description of one command.")]
            public partial class FirstCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd2", Description = "Description of another command.")]
            public partial class SecondCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd2 child", Description = "Description of another command's child command.")]
            public partial class SecondCommandChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd3 child", Description = "Description of an orphaned command.")]
            public partial class ThirdCommandChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommands(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();

        stdOut
            .Should()
            .ContainAllInOrder(
                "COMMANDS",
                "cmd1",
                "Description of one command.",
                "cmd2",
                "Description of another command.",
                // `cmd2 child` will appear as an immediate command because it does not
                // have a more specific parent.
                "cmd3 child",
                "Description of an orphaned command."
            );

        // `cmd2 child` will still appear in the list of `cmd2` subcommands,
        // but its description will not be visible.
        stdOut.Should().NotContain("Description of another command's child command.");
    }

    [Fact]
    public async Task I_can_request_the_help_text_to_see_the_list_of_all_immediate_grand_child_commands()
    {
        // Arrange
        var command = CommandCompiler.CompileMany(
            // lang=csharp
            """
            [Command("cmd1")]
            public partial class FirstCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd1 child1")]
            public partial class FirstCommandFirstChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd2")]
            public partial class SecondCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd2 child11")]
            public partial class SecondCommandFirstChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }

            [Command("cmd2 child2")]
            public partial class SecondCommandSecondChildCommand : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommands(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--help"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut
            .Should()
            .ContainAllInOrder(
                "COMMANDS",
                "cmd1",
                "Subcommands:",
                "cmd1 child1",
                "cmd2",
                "Subcommands:",
                "cmd2 child1",
                "cmd2 child2"
            );
    }

    [Fact]
    public async Task I_can_request_the_version_text_by_running_the_application_with_the_implicit_version_option()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommand(NoOpCommand.Descriptor)
            .SetVersion("v6.9")
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--version"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("v6.9");
    }
}
