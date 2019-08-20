using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Services;

namespace CliFx.Tests
{
    public partial class CliApplicationTests
    {
        [Command]
        private class DefaultCommand : ICommand
        {
            public Task ExecuteAsync(IConsole console)
            {
                console.Output.WriteLine("DefaultCommand executed.");
                return Task.CompletedTask;
            }
        }

        [Command("cmd")]
        private class NamedCommand : ICommand
        {
            public Task ExecuteAsync(IConsole console)
            {
                console.Output.WriteLine("NamedCommand executed.");
                return Task.CompletedTask;
            }
        }
    }

    // Negative
    public partial class CliApplicationTests
    {
        [Command("faulty1")]
        private class FaultyCommand1 : ICommand
        {
            public Task ExecuteAsync(IConsole console) => throw new CommandException(150);
        }

        [Command("faulty2")]
        private class FaultyCommand2 : ICommand
        {
            public Task ExecuteAsync(IConsole console) => throw new CommandException("FaultyCommand2 error message.", 150);
        }

        [Command("faulty3")]
        private class FaultyCommand3 : ICommand
        {
            public Task ExecuteAsync(IConsole console) => throw new Exception("FaultyCommand3 error message.");
        }
    }
}