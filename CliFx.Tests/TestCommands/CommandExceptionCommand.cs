using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;

namespace CliFx.Tests.TestCommands
{
    [Command("exc")]
    public class CommandExceptionCommand : ICommand
    {
        [CommandOption("code", 'c')]
        public int ExitCode { get; set; } = 1337;
        
        [CommandOption("msg", 'm')]
        public string? Message { get; set; }
        
        public ValueTask ExecuteAsync(IConsole console) => throw new CommandException(Message, ExitCode);
    }
}