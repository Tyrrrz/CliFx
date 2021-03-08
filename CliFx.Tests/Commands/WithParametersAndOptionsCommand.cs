using System.Collections.Generic;
using CliFx.Attributes;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class WithParametersAndOptionsCommand : SelfSerializeCommandBase
    {
        [CommandParameter(0)]
        public string? ParamA { get; set; }

        [CommandParameter(1)]
        public int? ParamB { get; set; }

        [CommandParameter(2)]
        public IReadOnlyList<string>? ParamC { get; set; }

        [CommandOption("opt-a", 'a', IsRequired = true)]
        public string? OptA { get; set; }

        [CommandOption("opt-b", 'b')]
        public int? OptB { get; set; }
    }
}