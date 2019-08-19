using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Services;

namespace CliFx.Tests
{
    public partial class CliApplicationTests
    {
        [Command]
        private class TestDefaultCommand : ICommand
        {
            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }

        [Command("command")]
        private class TestNamedCommand : ICommand
        {
            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }

        [Command("faulty command")]
        private class TestFaultyCommand : ICommand
        {
            public Task ExecuteAsync(IConsole console) => Task.FromException(new CommandException(-1337));
        }
    }
}