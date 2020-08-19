using System.Collections.Generic;
using CliFx.Attributes;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class WithParametersCommand : SelfSerializeCommandBase
    {
        [CommandParameter(0)]
        public string? ParamA { get; set; }

        [CommandParameter(1)]
        public int? ParamB { get; set; }

        [CommandParameter(2)]
        public IReadOnlyList<string>? ParamC { get; set; }
    }
}