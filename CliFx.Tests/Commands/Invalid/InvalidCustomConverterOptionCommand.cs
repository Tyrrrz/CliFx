using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    [Command]
    public class InvalidCustomConverterOptionCommand : SelfSerializeCommandBase
    {
        [CommandOption('f', Converter = typeof(Converter))]
        public string? Option { get; set; }

        public class Converter
        {
            public object ConvertFrom(string value) => value;
        }
    }
}