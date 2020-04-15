using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;

namespace CliFx.Tests
{
    public partial class HelpTextSpecs
    {
        [Command(Description = "DefaultCommand description.")]
        private class DefaultCommand : ICommand
        {
            [CommandOption("option-a", 'a', Description = "OptionA description.")]
            public string? OptionA { get; set; }

            [CommandOption("option-b", 'b', Description = "OptionB description.")]
            public string? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd", Description = "NamedCommand description.")]
        private class NamedCommand : ICommand
        {
            [CommandParameter(0, Name = "param-a", Description = "ParameterA description.")]
            public string? ParameterA { get; set; }

            [CommandOption("option-c", 'c', Description = "OptionC description.")]
            public string? OptionC { get; set; }

            [CommandOption("option-d", 'd', Description = "OptionD description.")]
            public string? OptionD { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd sub", Description = "NamedSubCommand description.")]
        private class NamedSubCommand : ICommand
        {
            [CommandParameter(0, Name = "param-b", Description = "ParameterB description.")]
            public string? ParameterB { get; set; }

            [CommandParameter(1, Name = "param-c", Description = "ParameterC description.")]
            public string? ParameterC { get; set; }

            [CommandOption("option-e", 'e', Description = "OptionE description.")]
            public string? OptionE { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd-with-params")]
        private class ParametersCommand : ICommand
        {
            [CommandParameter(0, Name = "first")]
            public string? ParameterA { get; set; }

            [CommandParameter(10)]
            public int? ParameterB { get; set; }

            [CommandParameter(20, Description = "A list of numbers", Name = "third list")]
            public IEnumerable<int>? ParameterC { get; set; }

            [CommandOption("option", 'o')]
            public string? Option { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd-with-req-opts")]
        private class RequiredOptionsCommand : ICommand
        {
            [CommandOption("option-f", 'f', IsRequired = true)]
            public string? OptionF { get; set; }

            [CommandOption("option-g", 'g', IsRequired = true)]
            public IEnumerable<int>? OptionG { get; set; }

            [CommandOption("option-h", 'h')]
            public string? OptionH { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd-with-env-vars")]
        private class EnvironmentVariableCommand : ICommand
        {
            [CommandOption("option-a", 'a', IsRequired = true, EnvironmentVariableName = "ENV_OPT_A")]
            public string? OptionA { get; set; }

            [CommandOption("option-b", 'b', EnvironmentVariableName = "ENV_OPT_B")]
            public string? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd")]
        private class ShowHelpTextOnCommandExceptionCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => throw new CommandException(CommandErrorDisplayOptions.HelpText);
        }

        [Command("cmd sub")]
        private class ShowHelpTextOnCommandExceptionSubCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd")]
        private class ShowErrorMessageThenHelpTextOnCommandExceptionCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => 
                throw new CommandException("Error message.",
                    CommandErrorDisplayOptions.ExceptionMessage | CommandErrorDisplayOptions.HelpText);
        }

        [Command("cmd sub")]
        private class ShowErrorMessageThenHelpTextOnCommandExceptionSubCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => default;
        }
    }
}