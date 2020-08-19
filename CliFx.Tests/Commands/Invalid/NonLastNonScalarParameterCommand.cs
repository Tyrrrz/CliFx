using System.Collections.Generic;
using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    [Command("cmd")]
    public class NonLastNonScalarParameterCommand : SelfSerializeCommandBase
    {
        [CommandParameter(0)]
        public IReadOnlyList<string>? ParamA { get; set; }

        [CommandParameter(1)]
        public string? ParamB { get; set; }
    }
}