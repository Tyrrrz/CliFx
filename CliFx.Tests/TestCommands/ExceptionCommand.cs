using System;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("exc")]
    public class ExceptionCommand : ICommand
    {
        [CommandOption("msg", 'm')]
        public string Message { get; set; }
        
        public Task ExecuteAsync(IConsole console, CancellationToken cancellationToken) => throw new Exception(Message);
    }
}