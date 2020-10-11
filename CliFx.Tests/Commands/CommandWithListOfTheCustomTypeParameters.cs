using CliFx.Attributes;
using CliFx.Tests.Converters;
using System.Collections.Generic;

namespace CliFx.Tests.Commands
{
    [Command("cmd", Description = "command with custom type description")]
    public class CommandWithListOfTheCustomTypeParameters : SelfSerializeCommandBase
    {
        [CommandOption("custom-type")]
        public List<CustomType>? Properties { get; set; }
    }
}
