using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public partial class ActivationSpecs
{
    public partial class Parameters(ITestOutputHelper testOutput) : SpecsBase(testOutput)
    {
        [Fact]
        public async Task I_can_pass_a_value_to_a_parameter()
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
            var exitCode = await application.RunAsync(
                ["one", "two"],
                new Dictionary<string, string>()
            );

            // Assert
            exitCode.Should().Be(0);

            var stdOut = FakeConsole.ReadOutputString();
            stdOut.Should().ConsistOfLines("Foo = one", "Bar = two");
        }

        [Fact]
        public async Task I_can_pass_multiple_values_to_a_sequence_based_parameter()
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
                .ConsistOfLines(
                    "Foo = one",
                    "Bar = two",
                    "Baz = three",
                    "Baz = four",
                    "Baz = five"
                );
        }

        [Fact]
        public async Task I_can_pass_nothing_to_a_non_required_parameter_to_keep_its_default_value()
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
        public async Task I_can_try_to_pass_nothing_to_a_required_parameter_and_get_an_error()
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
        public async Task I_can_try_to_pass_nothing_to_a_required_sequence_based_parameter_and_get_an_error()
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
        public async Task I_can_try_to_pass_too_many_values_to_parameters_and_get_an_error()
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
}
