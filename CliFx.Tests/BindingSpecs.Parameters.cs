using CliFx.Generators;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public partial class BindingSpecs
{
    public partial class Parameters(ITestOutputHelper testOutput) : SpecsBase(testOutput)
    {
        [Fact]
        public void I_can_try_to_bind_a_non_required_parameter_that_is_not_last_by_order_and_get_an_error()
        {
            // Act
            var act = () =>
                CommandCompiler.Compile(
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
                    """
                );

            // Assert
            act.Should()
                .Throw()
                .WithMessage(
                    $"*{DiagnosticDescriptors.CommandParameterMustHaveHighestOrderIfNotRequired.Id}*"
                );
        }

        [Fact]
        public void I_can_try_to_bind_a_sequence_based_parameter_that_is_not_last_by_order_and_get_an_error()
        {
            // Act
            var act = () =>
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandParameter(0)]
                        public required IReadOnlyList<string> Foo { get; set; }

                        [CommandParameter(1)]
                        public required string Bar { get; set; }

                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                );

            // Assert
            act.Should()
                .Throw()
                .WithMessage(
                    $"*{DiagnosticDescriptors.CommandParameterMustHaveHighestOrderIfSequenceBased.Id}*"
                );
        }

        [Fact]
        public void I_can_try_to_bind_a_parameter_to_a_non_enumerable_property_that_is_not_last_by_order_but_uses_a_sequence_converter_and_get_an_error()
        {
            // Act
            var act = () =>
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public class IntSequenceConverter : SequenceInputConverter<int>
                    {
                        public override int Convert(IReadOnlyList<string> rawValues) => rawValues.Count;
                    }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandParameter(0, Converter = typeof(IntSequenceConverter))]
                        public required int Foo { get; set; }

                        [CommandParameter(1)]
                        public required string Bar { get; set; }

                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                );

            // Assert
            act.Should()
                .Throw()
                .WithMessage(
                    $"*{DiagnosticDescriptors.CommandParameterMustHaveHighestOrderIfSequenceBased.Id}*"
                );
        }
    }
}
