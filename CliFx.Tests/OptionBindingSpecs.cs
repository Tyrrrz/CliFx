using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class OptionBindingSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public async Task I_can_bind_an_option_to_a_property_and_get_the_value_from_the_corresponding_argument_by_name()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo")]
                public bool Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_property_and_get_the_value_from_the_corresponding_argument_by_short_name()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption('f')]
                public bool Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_property_and_get_the_value_from_the_corresponding_argument_set_by_name()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo")]
                public string? Foo { get; init; }

                [CommandOption("bar")]
                public string? Bar { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("Foo = " + Foo);
                    console.WriteLine("Bar = " + Bar);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_property_and_get_the_value_from_the_corresponding_argument_set_by_short_name()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption('f')]
                public string? Foo { get; init; }

                [CommandOption('b')]
                public string? Bar { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("Foo = " + Foo);
                    console.WriteLine("Bar = " + Bar);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_property_and_get_the_value_from_the_corresponding_argument_stack_by_short_name()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption('f')]
                public string? Foo { get; init; }

                [CommandOption('b')]
                public string? Bar { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("Foo = " + Foo);
                    console.WriteLine("Bar = " + Bar);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_non_scalar_property_and_get_the_values_from_the_corresponding_arguments_by_name()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("Foo")]
                public IReadOnlyList<string>? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_non_scalar_property_and_get_the_values_from_the_corresponding_arguments_by_short_name()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption('f')]
                public IReadOnlyList<string>? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_non_scalar_property_and_get_the_values_from_the_corresponding_argument_sets_by_name()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo")]
                public IReadOnlyList<string>? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_non_scalar_property_and_get_the_values_from_the_corresponding_argument_sets_by_short_name()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption('f')]
                public IReadOnlyList<string>? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_non_scalar_property_and_get_the_values_from_the_corresponding_argument_sets_by_name_or_short_name()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo", 'f')]
                public IReadOnlyList<string>? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    foreach (var i in Foo)
                        console.WriteLine(i);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_property_and_get_no_value_if_the_user_does_not_provide_the_corresponding_argument()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo")]
                public string? Foo { get; init; }

                [CommandOption("bar")]
                public string? Bar { get; init; } = "hello";

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("Foo = " + Foo);
                    console.WriteLine("Bar = " + Bar);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_property_through_multiple_inheritance()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            public static class SharedContext
            {
                public static int Foo { get; set; }

                public static bool Bar { get; set; }
            }

            public interface IHasFoo : ICommand
            {
                [CommandOption("foo")]
                public int Foo
                {
                    get => SharedContext.Foo;
                    init => SharedContext.Foo = value;
                }
            }

            public interface IHasBar : ICommand
            {
                [CommandOption("bar")]
                public bool Bar
                {
                    get => SharedContext.Bar;
                    init => SharedContext.Bar = value;
                }
            }

            public interface IHasBaz : ICommand
            {
                public string? Baz { get; init; }
            }

            [Command]
            public class Command : IHasFoo, IHasBar, IHasBaz
            {
                [CommandOption("baz")]
                public string? Baz { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("Foo = " + SharedContext.Foo);
                    console.WriteLine("Bar = " + SharedContext.Bar);
                    console.WriteLine("Baz = " + Baz);

                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_property_and_get_the_correct_value_if_the_user_provides_an_argument_containing_a_negative_number()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo")]
                public string? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
    public async Task I_can_bind_an_option_to_a_property_with_the_same_identifier_as_the_implicit_help_option_and_get_the_correct_value()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("help", 'h')]
                public string? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["--help", "me"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("me");
    }

    [Fact]
    public async Task I_can_bind_an_option_to_a_property_with_the_same_identifier_as_the_implicit_version_option_and_get_the_correct_value()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("version")]
                public string? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine(Foo);
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["--version", "1.2.0"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("1.2.0");
    }

    [Fact]
    public async Task I_can_try_to_bind_a_required_option_to_a_property_and_get_an_error_if_the_user_does_not_provide_the_corresponding_argument()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo")]
                public required string Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo")]
                public required string Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo")]
                public required IReadOnlyList<string> Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo")]
                public string? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
        var commandType = DynamicCommandBuilder.Compile(
            // lang=csharp
            """
            [Command]
            public class Command : ICommand
            {
                [CommandOption("foo")]
                public string? Foo { get; init; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
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
        stdErr.Should().Contain("expects a single argument, but provided with multiple");
    }
}
