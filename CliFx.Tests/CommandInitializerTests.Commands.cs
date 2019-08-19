using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests
{
    public partial class CommandInitializerTests
    {
        [Command]
        private class TestCommand : ICommand
        {
            [CommandOption("int", 'i', IsRequired = true)]
            public int IntOption { get; set; } = 24;

            [CommandOption("str", 's')]
            public string StringOption { get; set; } = "foo bar";

            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }
    }
}