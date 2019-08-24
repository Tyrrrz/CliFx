using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.Services
{
    public partial class CommandInitializerTests
    {
        [Command]
        private class TestCommand : ICommand
        {
            [CommandOption("int", 'i', IsRequired = true)]
            public int Option1 { get; set; } = 24;

            [CommandOption("str", 's')]
            public string Option2 { get; set; } = "foo bar";

            [CommandOption('S')]
            public bool Option3 { get; set; }

            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }
    }
}