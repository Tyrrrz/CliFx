using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests
{
    public partial class HelpTextSpecs
    {
        [Command(Description = "HelpDefaultCommand description.")]
        private class HelpDefaultCommand : ICommand
        {
            [CommandOption("option-a", 'a', Description = "OptionA description.")]
            public string? OptionA { get; set; }

            [CommandOption("option-b", 'b', Description = "OptionB description.")]
            public string? OptionB { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd", Description = "HelpNamedCommand description.")]
        private class HelpNamedCommand : ICommand
        {
            [CommandOption("option-c", 'c', Description = "OptionC description.")]
            public string? OptionC { get; set; }

            [CommandOption("option-d", 'd', Description = "OptionD description.")]
            public string? OptionD { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command("cmd sub", Description = "HelpSubCommand description.")]
        private class HelpSubCommand : ICommand
        {
            [CommandOption("option-e", 'e', Description = "OptionE description.")]
            public string? OptionE { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }
    }
}