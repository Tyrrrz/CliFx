using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    [Command("cmd")]
    public class DuplicateOptionNamesCommand : SelfSerializeCommandBase
    {
        [CommandOption("fruits")]
        public string? Apples { get; set; }

        [CommandOption("fruits")]
        public string? Oranges { get; set; }
    }
}