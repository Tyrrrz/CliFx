using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    [Command("cmd")]
    public class NonLetterCharacterShortNameCommand : SelfSerializeCommandBase
    {
        [CommandOption('0')]
        public string? Apples { get; set; }
    }
}