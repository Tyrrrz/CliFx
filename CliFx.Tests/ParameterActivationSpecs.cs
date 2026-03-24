using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Generators;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class ParameterActivationSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public async Task I_can_activate_a_parameter_to_a_property_and_get_the_value_from_the_corresponding_argument()
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
        var exitCode = await application.RunAsync(["one", "two"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = one", "Bar = two");
    }

    [Fact]
    public async Task I_can_activate_a_parameter_to_a_non_scalar_property_and_get_values_from_the_remaining_non_option_arguments()
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

                [CommandOption("boo")]
                public string? Boo { get; set; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.WriteLine("Foo = " + Foo);
                    console.WriteLine("Bar = " + Bar);

                    foreach (var i in Baz)
                        console.WriteLine("Baz = " + i);

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
            ["one", "two", "three", "four", "five", "--boo", "xxx"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut
            .Should()
            .ConsistOfLines("Foo = one", "Bar = two", "Baz = three", "Baz = four", "Baz = five");
    }

    [Fact]
    public async Task I_can_try_to_bind_a_parameter_to_a_property_and_get_an_error_if_the_user_does_not_provide_the_corresponding_argument()
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

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["one"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Missing required parameter(s)");
    }

    [Fact]
    public async Task I_can_try_to_bind_a_parameter_to_a_non_scalar_property_and_get_an_error_if_the_user_does_not_provide_at_least_one_corresponding_argument()
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
                public required IReadOnlyList<string> Bar { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CommandLineApplicationBuilder()
            .AddCommand(command)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["one"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Missing required parameter(s)");
    }

    [Fact]
    public async Task I_can_bind_a_non_required_parameter_to_a_property_and_get_no_value_if_the_user_does_not_provide_the_corresponding_argument()
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
                public string? Bar { get; set; } = "xyz";

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
        var exitCode = await application.RunAsync(["abc"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = abc", "Bar = xyz");
    }

    [Fact]
    public void I_get_an_error_if_a_non_required_parameter_is_not_last_in_order()
    {
        // Arrange
        _ = CommandCompiler.CreateCompilation(
            // lang=csharp
            """
            [Command]
            public partial class Command : ICommand
            {
                [CommandParameter(0)]
                public string? Foo { get; set; }

                [CommandParameter(1)]
                public required string Bar { get; set; }

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """,
            out var diagnostics
        );

        // Assert
        diagnostics
            .Should()
            .ContainSingle(d =>
                d.Id == DiagnosticDescriptors.CommandParameterMustHaveHighestOrderIfNotRequired.Id
            )
            .Which.GetMessage()
            .Should()
            .Contain("Foo")
            .And.Contain("Bar");
    }

    [Fact]
    public async Task I_can_try_to_bind_parameters_and_get_an_error_if_the_user_provides_too_many_arguments()
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
            ["one", "two", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Unrecognized parameter(s)");
    }
}
