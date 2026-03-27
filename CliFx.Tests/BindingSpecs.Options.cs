using System;
using System.Linq;
using CliFx.Binding;
using CliFx.Generators;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public partial class BindingSpecs
{
    public partial class Options(ITestOutputHelper testOutput) : SpecsBase(testOutput)
    {
        [Fact]
        public void I_can_try_to_bind_an_option_that_has_an_invalid_name_and_get_an_error()
        {
            // Act
            var act = () =>
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption("1option")]
                        public string? Option { get; set; }

                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                );

            // Assert
            act.Should()
                .Throw()
                .WithMessage($"*{DiagnosticDescriptors.CommandOptionMustHaveValidName.Id}*");
        }

        [Fact]
        public void I_can_try_to_bind_an_option_that_has_an_invalid_short_name_and_get_an_error()
        {
            // Act
            var act = () =>
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('1')]
                        public string? Option { get; set; }

                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                );

            // Assert
            act.Should()
                .Throw()
                .WithMessage($"*{DiagnosticDescriptors.CommandOptionMustHaveValidShortName.Id}*");
        }

        [Fact]
        public void I_can_bind_an_option_that_shadows_the_built_in_help_option_and_the_help_option_is_not_auto_generated()
        {
            // Arrange
            var command = CommandCompiler.Compile(
                // lang=csharp
                """
                [Command]
                public partial class Command : ICommand
                {
                    [CommandOption("help")]
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
                    string.Equals(
                        o.Property.Name,
                        nameof(ICommandWithHelpOption.IsHelpRequested),
                        StringComparison.Ordinal
                    )
                );
        }

        [Fact]
        public void I_can_bind_an_option_that_shadows_the_built_in_version_option_and_the_version_option_is_not_auto_generated()
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
                        nameof(ICommandWithVersionOption.IsVersionRequested),
                        StringComparison.Ordinal
                    )
                );
        }

        [Fact]
        public void I_can_try_to_bind_an_option_that_shadows_the_built_in_help_option_and_get_a_warning()
        {
            // Act
            var act = () =>
                CommandCompiler.Compile(
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
                    treatWarningsAsErrors: true
                );

            // Assert
            act.Should()
                .Throw()
                .WithMessage($"*{DiagnosticDescriptors.CommandOptionShadowsBuiltInHelpOption.Id}*");
        }

        [Fact]
        public void I_can_try_to_bind_an_option_that_shadows_the_built_in_version_option_and_get_a_warning()
        {
            // Act
            var act = () =>
                CommandCompiler.Compile(
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
                    treatWarningsAsErrors: true
                );

            // Assert
            act.Should()
                .Throw()
                .WithMessage(
                    $"*{DiagnosticDescriptors.CommandOptionShadowsBuiltInVersionOption.Id}*"
                );
        }

        [Fact]
        public void I_can_bind_an_option_to_a_sequence_property_with_an_element_converter()
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
                        [CommandOption('f', Converter = typeof(IntConverter), IsElementConverter = true)]
                        public IReadOnlyList<int>? Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                );

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void I_can_try_to_bind_an_option_with_element_converter_that_is_sequence_based_and_get_an_error()
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
                        [CommandOption('f', Converter = typeof(IntSequenceConverter), IsElementConverter = true)]
                        public int[]? Foo { get; set; }

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
    }
}
