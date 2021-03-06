using CliFx.Attributes;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class WithGeneratedOptionNamesCommand : SelfSerializeCommandBase
    {
        [CommandOption(IsRequired = true)]
        public string? Option { get; set; }

        [CommandOption]
        public string? AnotherOption { get; set; }

        [CommandOption('n')]
        public string? NonGenerated { get; set; }
    }
}
