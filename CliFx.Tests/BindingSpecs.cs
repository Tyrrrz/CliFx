using System;
using CliFx.Generators;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public partial class BindingSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
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
