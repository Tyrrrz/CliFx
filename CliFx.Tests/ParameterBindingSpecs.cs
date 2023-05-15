using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class ParameterBindingSpecs : SpecsBase
{
    public ParameterBindingSpecs(ITestOutputHelper testOutput)
        : base(testOutput)
    {
    }

    [Fact]
    public async Task I_can_bind_a_parameter_to_a_property_and_get_the_value_from_the_corresponding_argument()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                [CommandParameter(0)]
                public required string Foo { get; init; }

                [CommandParameter(1)]
                public required string Bar { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("Foo = " + Foo);
                    console.Output.WriteLine("Bar = " + Bar);

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
            new[] {"one", "two"},
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines(
            "Foo = one",
            "Bar = two"
        );
    }

    [Fact]
    public async Task I_can_bind_a_parameter_to_a_non_scalar_property_and_get_values_from_the_remaining_non_option_arguments()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                [CommandParameter(0)]
                public required string Foo { get; init; }

                [CommandParameter(1)]
                public required string Bar { get; init; }

                [CommandParameter(2)]
                public required IReadOnlyList<string> Baz { get; init; }

                [CommandOption("boo")]
                public string? Boo { get; init; }

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("Foo = " + Foo);
                    console.Output.WriteLine("Bar = " + Bar);

                    foreach (var i in Baz)
                        console.Output.WriteLine("Baz = " + i);

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
            new[] {"one", "two", "three", "four", "five", "--boo", "xxx"},
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines(
            "Foo = one",
            "Bar = two",
            "Baz = three",
            "Baz = four",
            "Baz = five"
        );
    }

    [Fact]
    public async Task I_can_bind_a_parameter_to_a_property_and_get_an_error_if_the_user_does_not_provide_the_corresponding_argument()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                [CommandParameter(0)]
                public required string Foo { get; init; }

                [CommandParameter(1)]
                public required string Bar { get; init; }

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
            new[] {"one"},
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Missing required parameter(s)");
    }

    [Fact]
    public async Task I_can_bind_a_parameter_to_a_non_scalar_property_and_get_an_error_if_the_user_does_not_provide_at_least_one_corresponding_argument()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                [CommandParameter(0)]
                public required string Foo { get; init; }

                [CommandParameter(1)]
                public required IReadOnlyList<string> Bar { get; init; }

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
            new[] {"one"},
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Missing required parameter(s)");
    }

    [Fact]
    public async Task I_can_bind_a_non_required_parameter_to_a_property_and_get_no_value_if_the_user_does_not_provide_the_corresponding_argument()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                [CommandParameter(0)]
                public required string Foo { get; init; }

                [CommandParameter(1, IsRequired = false)]
                public string? Bar { get; init; } = "xyz";

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("Foo = " + Foo);
                    console.Output.WriteLine("Bar = " + Bar);

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
            new[] {"abc"},
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines(
            "Foo = abc",
            "Bar = xyz"
        );
    }

    [Fact]
    public async Task I_can_bind_parameters_and_get_an_error_if_the_user_provides_too_many_arguments()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                [CommandParameter(0)]
                public required string Foo { get; init; }

                [CommandParameter(1)]
                public required string Bar { get; init; }

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
            new[] {"one", "two", "three"},
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Unexpected parameter(s)");
    }
}