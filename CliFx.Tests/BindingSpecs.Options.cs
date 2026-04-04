using System;
using System.Linq;
using CliFx.Binding;
using CliFx.Generators;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests;

public partial class BindingSpecs
{
    [Fact]
    public void I_can_try_to_bind_an_option_with_an_invalid_name_and_get_an_error()
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
    public void I_can_try_to_bind_an_option_with_an_invalid_short_name_and_get_an_error()
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
    public void I_can_try_to_bind_an_option_without_a_name_or_short_name_and_get_an_error()
    {
        // Act
        var act = () =>
            CommandCompiler.Compile(
                // lang=csharp
                """
                [Command]
                public partial class Command : ICommand
                {
                    [CommandOption("")]
                    public string? Option { get; set; }

                    public ValueTask ExecuteAsync(IConsole console) => default;
                }
                """
            );

        // Assert
        act.Should()
            .Throw()
            .WithMessage($"*{DiagnosticDescriptors.CommandOptionMustHaveNameOrShortName.Id}*");
    }

    [Fact]
    public void I_can_bind_an_option_that_shadows_the_built_in_help_option_and_the_help_option_is_not_auto_generated()
    {
        // Arrange
        var command = CommandCompiler
            .Compile(
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
            )
            .Single();

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
        var command = CommandCompiler
            .Compile(
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
            )
            .Single();

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
            .WithMessage(
                $"*{DiagnosticDescriptors.CommandOptionShadowsConventionalHelpOption.Id}*"
            );
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
                $"*{DiagnosticDescriptors.CommandOptionShadowsConventionalVersionOption.Id}*"
            );
    }

    [Fact]
    public void I_can_try_to_implement_the_help_option_without_an_input_binding_and_get_an_error()
    {
        // Act
        var act = () =>
            CommandCompiler.Compile(
                // lang=csharp
                """
                [Command]
                public partial class Command : ICommand, ICommandWithHelpOption
                {
                    public bool IsHelpRequested { get; set; }

                    public ValueTask ExecuteAsync(IConsole console) => default;
                }
                """
            );

        // Assert
        act.Should()
            .Throw()
            .WithMessage($"*{DiagnosticDescriptors.CommandHelpOptionPropertyMustBeBound.Id}*");
    }

    [Fact]
    public void I_can_try_to_implement_the_version_option_without_an_input_binding_and_get_an_error()
    {
        // Act
        var act = () =>
            CommandCompiler.Compile(
                // lang=csharp
                """
                [Command]
                public partial class Command : ICommand, ICommandWithVersionOption
                {
                    public bool IsVersionRequested { get; set; }

                    public ValueTask ExecuteAsync(IConsole console) => default;
                }
                """
            );

        // Assert
        act.Should()
            .Throw()
            .WithMessage($"*{DiagnosticDescriptors.CommandVersionOptionPropertyMustBeBound.Id}*");
    }

    [Fact]
    public void I_can_try_to_bind_two_options_with_the_same_name_and_get_an_error()
    {
        // Act
        var act = () =>
            CommandCompiler.Compile(
                // lang=csharp
                """
                [Command]
                public partial class Command : ICommand
                {
                    [CommandOption("foo")]
                    public string? Foo { get; set; }

                    [CommandOption("foo")]
                    public string? Bar { get; set; }

                    public ValueTask ExecuteAsync(IConsole console) => default;
                }
                """
            );

        // Assert
        act.Should()
            .Throw()
            .WithMessage($"*{DiagnosticDescriptors.CommandOptionMustHaveUniqueName.Id}*");
    }

    [Fact]
    public void I_can_try_to_bind_two_options_with_the_same_short_name_and_get_an_error()
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
                    public string? Foo { get; set; }

                    [CommandOption('f')]
                    public string? Bar { get; set; }

                    public ValueTask ExecuteAsync(IConsole console) => default;
                }
                """
            );

        // Assert
        act.Should()
            .Throw()
            .WithMessage($"*{DiagnosticDescriptors.CommandOptionMustHaveUniqueShortName.Id}*");
    }
}
