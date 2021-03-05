using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class GenericInnerExceptionCommand : ICommand
    {
        [CommandOption("msg", 'm')]
        public string? Message { get; set; }

        [CommandOption("inner-msg", 'i')]
        public string? InnerMessage { get; set; }

        public ValueTask ExecuteAsync(IConsole console) =>
            throw new Exception(Message, new Exception(InnerMessage));
    }
}