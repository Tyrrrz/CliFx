using CliFx.Attributes;
using CliFx.Tests.Converters;

namespace CliFx.Tests.Commands
{
    [Command("cmd", Description = "command with custom type description")]
    public class CommandWithCustomTypeParameter : SelfSerializeCommandBase
    {
        [CommandOption("custom-type")]
        public CustomType? Property { get; set; }
    }
}
