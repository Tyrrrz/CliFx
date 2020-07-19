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
            public int ExitCode { get; set; } = 133;

            [CommandOption("msg", 'm')]
            public string? Message { get; set; }

            [CommandOption("show-help")]
            public bool ShowHelp { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => throw new CommandException(Message, ExitCode, ShowHelp);
        }
    }
}