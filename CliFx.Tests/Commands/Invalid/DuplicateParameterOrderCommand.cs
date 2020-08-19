using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    [Command("cmd")]
    public class DuplicateParameterOrderCommand : SelfSerializeCommandBase
    {
        [CommandParameter(13)]
        public string? ParamA { get; set; }

        [CommandParameter(13)]
        public string? ParamB { get; set; }
    }
}