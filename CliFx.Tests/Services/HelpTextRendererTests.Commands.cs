using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.Services
{
    public partial class HelpTextRendererTests
    {
        [Command(Description = "DefaultCommand description.")]
        private class DefaultCommand : ICommand
        {
            [CommandOption("option-a", 'a', Description = "OptionA description.")]
            public string OptionA { get; set; }

            [CommandOption("option-b", 'b', Description = "OptionB description.")]
            public string OptionB { get; set; }

            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }

        [Command("cmd", Description = "NamedCommand description.")]
        private class NamedCommand : ICommand
        {
            [CommandOption("option-c", 'c', Description = "OptionC description.")]
            public string OptionC { get; set; }

            [CommandOption("option-d", 'd', Description = "OptionD description.")]
            public string OptionD { get; set; }

            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }

        [Command("cmd sub", Description = "NamedSubCommand description.")]
        private class NamedSubCommand : ICommand
        {
            [CommandOption("option-e", 'e', Description = "OptionE description.")]
            public string OptionE { get; set; }

            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }
    }
}