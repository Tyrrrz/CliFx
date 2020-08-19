using CliFx.Attributes;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class WithEnumArgumentsCommand : SelfSerializeCommandBase
    {
        public enum CustomEnum { Value1, Value2, Value3 };

        [CommandParameter(0, Name = "enum")]
        public CustomEnum EnumParameter { get; set; }

        [CommandOption("enum")]
        public CustomEnum? EnumOption { get; set; }

        [CommandOption("required-enum", IsRequired = true)]
        public CustomEnum RequiredEnumOption { get; set; }
    }
}