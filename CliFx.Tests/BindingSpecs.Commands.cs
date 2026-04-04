using CliFx.Generators;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests;

public partial class BindingSpecs
{
    [Fact]
    public void I_can_try_to_bind_a_command_to_a_type_that_is_not_partial_and_get_an_error()
    {
        // Act
        var act = () =>
            CommandCompiler.Compile(
                // lang=csharp
                """
                [Command]
                public class Command : ICommand
                {
                    public ValueTask ExecuteAsync(IConsole console) => default;
                }
                """
            );

        // Assert
        act.Should().Throw().WithMessage($"*{DiagnosticDescriptors.CommandMustBePartial.Id}*");
    }

    [Fact]
    public void I_can_try_to_bind_a_command_to_a_type_that_does_not_implement_ICommand_and_get_an_error()
    {
        // Act
        var act = () =>
            CommandCompiler.Compile(
                // lang=csharp
                """
                [Command]
                public partial class Command
                {
                    public ValueTask ExecuteAsync(IConsole console) => default;
                }
                """
            );

        // Assert
        act.Should()
            .Throw()
            .WithMessage($"*{DiagnosticDescriptors.CommandMustImplementICommand.Id}*");
    }

    [Fact]
    public void I_can_try_to_bind_two_commands_with_the_same_name_and_get_an_error()
    {
        // Act
        var act = () =>
            CommandCompiler.Compile(
                // lang=csharp
                """
                [Command("cmd")]
                public partial class FirstCommand : ICommand
                {
                    public ValueTask ExecuteAsync(IConsole console) => default;
                }

                [Command("cmd")]
                public partial class SecondCommand : ICommand
                {
                    public ValueTask ExecuteAsync(IConsole console) => default;
                }
                """
            );

        // Assert
        act.Should()
            .Throw()
            .WithMessage($"*{DiagnosticDescriptors.CommandMustHaveUniqueName.Id}*");
    }
}
