using System;
using System.Linq;
using CliFx.Binding;
using CliFx.Generators;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests.Binding;

public class OptionBindingSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public void I_can_bind_an_option_input_that_shadows_the_built_in_help_option_and_the_help_option_is_not_auto_generated()
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
    public void I_can_bind_an_option_input_that_shadows_the_built_in_version_option_and_the_version_option_is_not_auto_generated()
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
    public void I_can_try_to_bind_an_option_input_that_shadows_the_built_in_help_option_and_get_a_warning()
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
            .WithMessage(
                $"*{nameof(DiagnosticDescriptors.CommandOptionShadowsBuiltInHelpOption)}*"
            );
    }

    [Fact]
    public void I_can_try_to_bind_an_option_input_that_shadows_the_built_in_version_option_and_get_a_warning()
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
                $"*{nameof(DiagnosticDescriptors.CommandOptionShadowsBuiltInVersionOption)}*"
            );
    }
}
