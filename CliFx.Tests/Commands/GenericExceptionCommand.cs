using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class GenericExceptionCommand : ICommand
    {
        [CommandOption("msg", 'm')]
        public string? Message { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => throw new Exception(Message);
    }
}