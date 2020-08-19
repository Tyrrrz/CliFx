using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class CommandExceptionCommand : ICommand
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