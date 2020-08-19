using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    [Command("cmd")]
    public class DuplicateOptionShortNamesCommand : SelfSerializeCommandBase
    {
        [CommandOption('x')]
        public string? OptionA { get; set; }

        [CommandOption('x')]
        public string? OptionB { get; set; }
    }
}