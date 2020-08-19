using CliFx.Attributes;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class WithSingleRequiredOptionCommand : SelfSerializeCommandBase
    {
        [CommandOption("opt-a")]
        public string? OptA { get; set; }

        [CommandOption("opt-b", IsRequired = true)]
        public string? OptB { get; set; }
    }
}