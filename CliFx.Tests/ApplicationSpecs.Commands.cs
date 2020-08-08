using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests
{
    public partial class ApplicationSpecs
    {
        [Command]
        private class DefaultCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class AnotherDefaultCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class NonImplementedCommand
        {
        }

        private class NonAnnotatedCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("dup")]
        private class DuplicateNameCommandA : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("dup")]
        private class DuplicateNameCommandB : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class DuplicateParameterOrderCommand : ICommand
        {
            [CommandParameter(13)]
            public string? ParameterA { get; set; }

            [CommandParameter(13)]
            public string? ParameterB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class DuplicateParameterNameCommand : ICommand
        {
            [CommandParameter(0, Name = "param")]
            public string? ParameterA { get; set; }

            [CommandParameter(1, Name = "param")]
            public string? ParameterB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class MultipleNonScalarParametersCommand : ICommand
        {
            [CommandParameter(0)]
            public IReadOnlyList<string>? ParameterA { get; set; }

            [CommandParameter(1)]
            public IReadOnlyList<string>? ParameterB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class NonLastNonScalarParameterCommand : ICommand
        {
            [CommandParameter(0)]
            public IReadOnlyList<string>? ParameterA { get; set; }

            [CommandParameter(1)]
            public string? ParameterB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class EmptyOptionNameCommand : ICommand
        {
            [CommandOption("")]
            public string? Apples { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class SingleCharacterOptionNameCommand : ICommand
        {
            [CommandOption("a")]
            public string? Apples { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class DuplicateOptionNamesCommand : ICommand
        {
            [CommandOption("fruits")]
            public string? Apples { get; set; }

            [CommandOption("fruits")]
            public string? Oranges { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class DuplicateOptionShortNamesCommand : ICommand
        {
            [CommandOption('x')]
            public string? OptionA { get; set; }

            [CommandOption('x')]
            public string? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class ConflictWithHelpOptionCommand : ICommand
        {
            [CommandOption("option-h", 'h')]
            public string? OptionH { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class ConflictWithVersionOptionCommand : ICommand
        {
            [CommandOption("version")]
            public string? Version { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class DuplicateOptionEnvironmentVariableNamesCommand : ICommand
        {
            [CommandOption("option-a", EnvironmentVariableName = "ENV_VAR")]
            public string? OptionA { get; set; }

            [CommandOption("option-b", EnvironmentVariableName = "ENV_VAR")]
            public string? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("hidden", Description = "Description", Manual = "Manual", InteractiveModeOnly = false)]
        private class HiddenPropertiesCommand : ICommand
        {
            [CommandParameter(13, Name = "param", Description = "Param description")]
            public string? Parameter { get; set; }

            [CommandOption("option", 'o', Description = "Option description", EnvironmentVariableName = "ENV")]
            public string? Option { get; set; }

            public string? HiddenA { get; set; }

            public bool? HiddenB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }
    }
}