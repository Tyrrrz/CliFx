using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Generators;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class OptionActivationSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public async Task I_can_activate_an_option_to_a_property_and_get_the_value_from_the_corresponding_argument_by_name()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo")]
                public bool Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--foo"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("True");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_property_and_get_the_value_from_the_corresponding_argument_by_short_name()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption('f')]
                public bool Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["-f"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("True");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_property_and_get_the_value_from_the_corresponding_argument_set_by_name()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo")]
                public string? Foo { get; set; }

                [CommandOption("bar")]
                public string? Bar { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("Foo = " + Foo);
                    console.WriteLine("Bar = " + Bar);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["--foo", "one", "--bar", "two"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = one", "Bar = two");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_property_and_get_the_value_from_the_corresponding_argument_set_by_short_name()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption('f')]
                public string? Foo { get; set; }

                [CommandOption('b')]
                public string? Bar { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("Foo = " + Foo);
                    console.WriteLine("Bar = " + Bar);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "one", "-b", "two"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = one", "Bar = two");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_property_and_get_the_value_from_the_corresponding_argument_stack_by_short_name()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption('f')]
                public string? Foo { get; set; }

                [CommandOption('b')]
                public string? Bar { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("Foo = " + Foo);
                    console.WriteLine("Bar = " + Bar);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-fb", "value"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = ", "Bar = value");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_non_scalar_property_and_get_the_values_from_the_corresponding_arguments_by_name()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("Foo")]
                public IReadOnlyList<string>? Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["--foo", "one", "two", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("one", "two", "three");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_non_scalar_property_and_get_the_values_from_the_corresponding_arguments_by_short_name()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption('f')]
                public IReadOnlyList<string>? Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "one", "two", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("one", "two", "three");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_non_scalar_property_and_get_the_values_from_the_corresponding_argument_sets_by_name()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo")]
                public IReadOnlyList<string>? Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["--foo", "one", "--foo", "two", "--foo", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("one", "two", "three");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_non_scalar_property_and_get_the_values_from_the_corresponding_argument_sets_by_short_name()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption('f')]
                public IReadOnlyList<string>? Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "one", "-f", "two", "-f", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("one", "two", "three");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_non_scalar_property_and_get_the_values_from_the_corresponding_argument_sets_by_name_or_short_name()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo", 'f')]
                public IReadOnlyList<string>? Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["--foo", "one", "-f", "two", "--foo", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("one", "two", "three");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_property_and_get_no_value_if_the_user_does_not_provide_the_corresponding_argument()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo")]
                public string? Foo { get; set; }

                [CommandOption("bar")]
                public string? Bar { get; set; } = "hello";

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("Foo = " + Foo);
                    console.WriteLine("Bar = " + Bar);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["--foo", "one"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = one", "Bar = hello");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_property_through_multiple_inheritance()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            public interface IHasFoo : ICommand
            {
            }

            public interface IHasBar : ICommand
            {
            }

            public interface IHasBaz : ICommand
            {
                public string? Baz { get; set; }
            }

            [Command]
            public partial class Command : IHasFoo, IHasBar, IHasBaz
            {
                [CommandOption("foo")]
                public int Foo { get; set; }

                [CommandOption("bar")]
                public bool Bar { get; set; }

                [CommandOption("baz")]
                public string? Baz { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("Foo = " + Foo);
                    console.WriteLine("Bar = " + Bar);
                    console.WriteLine("Baz = " + Baz);

                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--foo", "42", "--bar", "--baz", "xyz"]);

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = 42", "Bar = True", "Baz = xyz");
    }

    [Fact]
    public async Task I_can_activate_an_option_to_a_property_and_get_the_correct_value_if_the_user_provides_an_argument_containing_a_negative_number()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo")]
                public string? Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    return default;
                }
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["--foo", "-13"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("-13");
    }

    [Fact]
    public async Task I_can_try_to_bind_a_required_option_to_a_property_and_get_an_error_if_the_user_does_not_provide_the_corresponding_argument()
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

                public ValueTask ExecuteAsync(IConsole console) => default;
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

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Missing required option(s)");
    }

    [Fact]
    public async Task I_can_try_to_bind_a_required_option_to_a_property_and_get_an_error_if_the_user_provides_an_empty_argument()
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

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--foo"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Missing required option(s)");
    }

    [Fact]
    public async Task I_can_try_to_bind_an_option_to_a_non_scalar_property_and_get_an_error_if_the_user_does_not_provide_at_least_one_corresponding_argument()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo")]
                public required IReadOnlyList<string> Foo { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["--foo"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Missing required option(s)");
    }

    [Fact]
    public async Task I_can_try_to_bind_options_and_get_an_error_if_the_user_provides_unrecognized_arguments()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo")]
                public string? Foo { get; set; }

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
            ["--foo", "one", "--bar", "two"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Unrecognized option(s)");
    }

    [Fact]
    public async Task I_can_try_to_bind_an_option_to_a_scalar_property_and_get_an_error_if_the_user_provides_too_many_arguments()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("foo")]
                public string? Foo { get; set; }

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
            ["--foo", "one", "two", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Expected a single argument, but provided with multiple");
    }

    [Fact]
    public void I_get_a_warning_if_a_user_option_partially_shadows_the_built_in_help_option()
    {
        // Arrange
        _ = CommandCompiler.CreateCompilation(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption('h')]
                public bool CustomHelp { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """,
            out var diagnostics
        );

        // Assert
        diagnostics
            .Should()
            .ContainSingle(d =>
                d.Id == DiagnosticDescriptors.CommandOptionShadowsBuiltInHelpOption.Id
            )
            .Which.GetMessage()
            .Should()
            .Contain("CustomHelp")
            .And.Contain("conventional help option")
            .And.Contain("via '-h'")
            .And.Contain("Consider choosing a different identifier");
    }

    [Fact]
    public void I_get_a_warning_if_a_user_option_completely_shadows_the_built_in_help_option()
    {
        // Arrange
        _ = CommandCompiler.CreateCompilation(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("help", 'h')]
                public bool CustomHelp { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """,
            out var diagnostics
        );

        // Assert
        diagnostics
            .Should()
            .ContainSingle(d =>
                d.Id == DiagnosticDescriptors.CommandOptionShadowsBuiltInHelpOption.Id
            )
            .Which.GetMessage()
            .Should()
            .Contain("CustomHelp")
            .And.Contain("conventional help option")
            .And.Contain("via '-h'")
            .And.Contain("Consider choosing a different identifier");
    }

    [Fact]
    public void I_get_a_warning_if_a_user_option_partially_shadows_the_built_in_version_option()
    {
        // Arrange
        _ = CommandCompiler.CreateCompilation(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("version")]
                public bool CustomVersion { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """,
            out var diagnostics
        );

        // Assert
        diagnostics
            .Should()
            .ContainSingle(d =>
                d.Id == DiagnosticDescriptors.CommandOptionShadowsBuiltInVersionOption.Id
            )
            .Which.GetMessage()
            .Should()
            .Contain("CustomVersion")
            .And.Contain("conventional version option")
            .And.Contain("via '--version'")
            .And.Contain("Consider choosing a different identifier");
    }

    [Fact]
    public void I_get_a_warning_if_a_user_option_completely_shadows_the_built_in_version_option()
    {
        // Arrange
        _ = CommandCompiler.CreateCompilation(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("version", 'v')]
                public bool CustomVersion { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """,
            out var diagnostics
        );

        // Assert
        diagnostics
            .Should()
            .ContainSingle(d =>
                d.Id == DiagnosticDescriptors.CommandOptionShadowsBuiltInVersionOption.Id
            )
            .Which.GetMessage()
            .Should()
            .Contain("CustomVersion")
            .And.Contain("conventional version option")
            .And.Contain("via '--version'")
            .And.Contain("Consider choosing a different identifier");
    }

    [Fact]
    public void I_do_not_get_a_warning_if_a_user_option_uses_the_short_name_v()
    {
        // Arrange
        _ = CommandCompiler.CreateCompilation(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption('v')]
                public bool CustomVersionShort { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """,
            out var diagnostics
        );

        // Assert
        diagnostics
            .Should()
            .NotContain(d =>
                d.Id == DiagnosticDescriptors.CommandOptionShadowsBuiltInVersionOption.Id
            );
    }

    [Fact]
    public void I_do_not_auto_generate_the_conventional_help_option_when_it_is_partially_shadowed()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption('h')]
                public bool CustomHelp { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        // Assert
        command
            .Inputs.OfType<CommandOptionDescriptor>()
            .Should()
            .NotContain(o =>
                string.Equals(o.Property.Name, "IsHelpRequested", System.StringComparison.Ordinal)
            );
    }

    [Fact]
    public void I_do_not_auto_generate_the_conventional_version_option_when_it_is_partially_shadowed()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandOption("version")]
                public bool CustomVersion { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        // Assert
        command
            .Inputs.OfType<CommandOptionDescriptor>()
            .Should()
            .NotContain(o =>
                string.Equals(
                    o.Property.Name,
                    "IsVersionRequested",
                    System.StringComparison.Ordinal
                )
            );
    }
}
