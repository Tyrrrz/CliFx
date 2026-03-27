using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Generators;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public partial class BindingSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public async Task I_can_bind_inputs_to_properties_defined_in_parent_types()
    {
        // Arrange
        var command = CommandCompiler.Compile(
            // lang=csharp
            """
            public abstract class GrandParentCommand : ICommand
            {
                [CommandParameter(0)]
                public string? Foo { get; set; }

                public abstract ValueTask ExecuteAsync(IConsole console);
            }

            public abstract class ParentCommand : GrandParentCommand
            {
                [CommandOption("bar")]
                public string? Bar { get; set; }
            }

            [Command]
            public partial class Command : ParentCommand
            {
                [CommandOption("baz")]
                public string? Baz { get; set; }

                public override ValueTask ExecuteAsync(IConsole console)
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
        var exitCode = await application.RunAsync(
            ["one", "--bar", "two", "--baz", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = one", "Bar = two", "Baz = three");
    }

    [Fact]
    public void I_can_try_to_bind_an_input_to_a_property_of_an_unsupported_type_and_get_an_error()
    {
        // Act
        var act = () =>
            CommandCompiler.Compile(
                // lang=csharp
                """
                public class CustomType
                {
                }

                [Command]
                public partial class Command : ICommand
                {
                    [CommandOption('f')]
                    public CustomType? Foo { get; set; }

                    public ValueTask ExecuteAsync(IConsole console) => default;
                }
                """
            );

        // Assert
        act.Should()
            .Throw()
            .WithMessage($"*{DiagnosticDescriptors.CommandInputConverterNotInferrable.Id}*");
    }

    [Fact]
    public void I_can_try_to_bind_an_input_to_a_sequence_based_property_of_an_unsupported_type_and_get_an_error()
    {
        // Act
        var act = () =>
            CommandCompiler.Compile(
                // lang=csharp
                """
                public class CustomType : IEnumerable<object>
                {
                    public IEnumerator<object> GetEnumerator() => Enumerable.Empty<object>().GetEnumerator();

                    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                }

                [Command]
                public partial class Command : ICommand
                {
                    [CommandOption('f')]
                    public CustomType? Foo { get; set; }

                    public ValueTask ExecuteAsync(IConsole console) => default;
                }
                """
            );

        // Assert
        act.Should()
            .Throw()
            .WithMessage($"*{DiagnosticDescriptors.CommandInputConverterNotInferrable.Id}*");
    }

    [Fact]
    public void I_can_try_to_bind_an_input_to_a_sequence_based_property_of_an_unsupported_interface_type_and_get_an_error()
    {
        // Act
        var act = () =>
            CommandCompiler.Compile(
                // lang=csharp
                """
                [Command]
                public partial class Command : ICommand
                {
                    [CommandOption('f')]
                    public ISet<string>? Foo { get; set; }

                    public ValueTask ExecuteAsync(IConsole console) => default;
                }
                """
            );

        // Assert
        act.Should()
            .Throw()
            .WithMessage($"*{DiagnosticDescriptors.CommandInputConverterNotInferrable.Id}*");
    }
}
