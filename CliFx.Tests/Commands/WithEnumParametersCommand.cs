using CliFx.Attributes;
using System.Collections.Generic;

namespace CliFx.Tests.Commands
{

    [Command("cmd")]
    public class WithEnumParametersCommand : SelfSerializeCommandBase
    {
        public enum CustomEnum { Value, Value1, Value2, Value3 };

        public enum CustomEnum2 { Custom, Custom1, Custom2, Custom3 };

        [CommandParameter(0, Name = "enum1")]
        public CustomEnum EnumParameter1 { get; set; }


        [CommandParameter(1, Name = "enum2")]
        public CustomEnum2 EnumParameter2 { get; set; }

        [CommandParameter(2, Name = "str1")]
        public string? StringParameter1 { get; set; }


        [CommandParameter(3, Name = "enumerable1")]
        public IEnumerable<string>? StringParameters { get; set; }
    }
}