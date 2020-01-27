using System;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command("exc")]
    public class ExceptionCommand : ICommand
    {
        [CommandOption("msg", 'm')]
        public string? Message { get; set; }
        
        public ValueTask ExecuteAsync(IConsole console) => throw new Exception(Message);
    }
}