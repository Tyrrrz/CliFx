using CliFx.Attributes;
using System;
using System.Threading.Tasks;
using CliFx.Tests.Commands.Converters;
using System.Collections.Generic;

namespace CliFx.Tests.Commands
{
    public class CustomType
    {
        public int SomeValue { get; set; }
    }

    [Command("cmd")]
    public class CommandWithParameterOfCustomType : SelfSerializeCommandBase
    {
        [CommandOption("prop", Converter = typeof(CustomTypeConverter))]
        public CustomType MyProperty { get; set; }
    }

    [Command("cmd")]
    public class CommandWithEnumerableOfParametersOfCustomType : SelfSerializeCommandBase
    {
        [CommandOption("prop", Converter = typeof(CustomTypeConverter))]
        public List<CustomType> MyProperties { get; set; }
    }
}
