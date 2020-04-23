using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;

namespace CliFx.Tests
{
    public partial class ErrorReportingSpecs
    {
        [Command("exc")]
        private class GenericExceptionCommand : ICommand
        {
            [CommandOption("msg", 'm')]
            public string? Message { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => throw new Exception(Message);
        }

        [Command("exc")]
        private class CommandExceptionCommand : ICommand
        {
            [CommandOption("code", 'c')]
            public int ExitCode { get; set; } = 1337;

            [CommandOption("msg", 'm')]
            public string? Message { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => throw new CommandException(Message, ExitCode);
        }

        [Command("exc")]
        private class ShowHelpTextOnlyCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => throw new CommandException(null, showHelp: true);
        }

        [Command("exc sub")]
        private class ShowHelpTextOnlySubCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("exc")]
        private class ShowErrorMessageThenHelpTextCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) =>
                throw new CommandException("Error message.", showHelp: true);
        }

        [Command("exc sub")]
        private class ShowErrorMessageThenHelpTextSubCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("exc")]
        private class StackTraceOnlyCommand : ICommand
        {
            [CommandOption("msg", 'm')]
            public string? Message { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => throw new CommandException(null);
        }
    }
}