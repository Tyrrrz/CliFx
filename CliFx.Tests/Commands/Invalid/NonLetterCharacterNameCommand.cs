using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    [Command("cmd")]
    public class NonLetterCharacterNameCommand : SelfSerializeCommandBase
    {
        [CommandOption("0foo")]
        public string? Apples { get; set; }
    }
}