using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    [Command]
    public class InvalidCustomConverterParameterCommand : SelfSerializeCommandBase
    {
        [CommandParameter(0, Converter = typeof(Converter))]
        public string? Param { get; set; }

        public class Converter
        {
            public object ConvertFrom(string value) => value;
        }
    }
}