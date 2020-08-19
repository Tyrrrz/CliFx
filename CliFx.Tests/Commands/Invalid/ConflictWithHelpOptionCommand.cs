using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    [Command("cmd")]
    public class ConflictWithHelpOptionCommand : SelfSerializeCommandBase
    {
        [CommandOption("option-h", 'h')]
        public string? OptionH { get; set; }
    }
}