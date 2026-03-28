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

        [Fact]
        public void I_can_bind_a_parameter_to_a_sequence_property_with_an_element_converter()
        {
            // Act
            var act = () =>
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public class IntConverter : ScalarInputConverter<int>
                    {
                        public override int Convert(string? rawValue) =>
                            int.Parse(rawValue!, CultureInfo.InvariantCulture);
                    }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandParameter(0, Converter = typeof(IntConverter), IsElementConverter = true)]
                        public required IReadOnlyList<int> Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                );

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void I_can_try_to_bind_a_parameter_with_element_converter_that_is_sequence_based_and_get_an_error()
        {
            // Act
            var act = () =>
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public class IntSequenceConverter : SequenceInputConverter<int[]>
                    {
                        public override int[] Convert(IReadOnlyList<string> rawValues) =>
                            rawValues.Select(v => int.Parse(v, CultureInfo.InvariantCulture)).ToArray();
                    }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandParameter(0, Converter = typeof(IntSequenceConverter), IsElementConverter = true)]
                        public required int[] Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                );

            // Assert
            act.Should()
                .Throw()
                .WithMessage(
                    $"*{DiagnosticDescriptors.CommandInputElementConverterMustNotBeSequenceBased.Id}*"
                );
        }

        [Fact]
        public void I_can_bind_a_parameter_with_element_converter_and_it_is_treated_as_sequence_based()
        {
            // A parameter with IsElementConverter = true on a sequence property should be
            // treated as sequence-based, meaning it must be the last parameter by order.
            // Act
            var act = () =>
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public class IntConverter : ScalarInputConverter<int>
                    {
                        public override int Convert(string? rawValue) =>
                            int.Parse(rawValue!, CultureInfo.InvariantCulture);
                    }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandParameter(0, Converter = typeof(IntConverter), IsElementConverter = true)]
                        public required IReadOnlyList<int> Foo { get; set; }

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
        public void I_can_try_to_bind_a_parameter_with_element_converter_on_a_non_sequence_property_and_get_an_error()
        {
            // Act
            var act = () =>
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public class IntConverter : ScalarInputConverter<int>
                    {
                        public override int Convert(string? rawValue) =>
                            int.Parse(rawValue!, CultureInfo.InvariantCulture);
                    }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandParameter(0, Converter = typeof(IntConverter), IsElementConverter = true)]
                        public required int Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                );

            // Assert
            act.Should()
                .Throw()
                .WithMessage(
                    $"*{DiagnosticDescriptors.CommandInputElementConverterRequiresSequenceProperty.Id}*"
                );
        }
    }
}
