using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests
{
    public partial class CommandSchemaResolverTests
    {
        [Command("Command name", Description = "Command description")]
        private class TestCommand : ICommand
        {
            [CommandOption("option-a", 'a')]
            public int OptionA { get; set; }

            [CommandOption("option-b", IsRequired = true)]
            public string OptionB { get; set; }

            [CommandOption("option-c", Description = "Option C description")]
            public bool OptionC { get; set; }

            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }
    }
}