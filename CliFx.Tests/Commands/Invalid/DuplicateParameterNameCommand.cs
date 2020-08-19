using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    [Command("cmd")]
    public class DuplicateParameterNameCommand : SelfSerializeCommandBase
    {
        [CommandParameter(0, Name = "param")]
        public string? ParamA { get; set; }

        [CommandParameter(1, Name = "param")]
        public string? ParamB { get; set; }
    }
}