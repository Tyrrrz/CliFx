using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    // Must be default because version option is available only on default commands
    [Command]
    public class ConflictWithVersionOptionCommand : SelfSerializeCommandBase
    {
        [CommandOption("version")]
        public string? Version { get; set; }
    }
}