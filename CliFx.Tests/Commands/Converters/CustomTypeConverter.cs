using CliFx.Domain;

namespace CliFx.Tests.Commands.Converters
{
    public class CustomTypeConverter : IConverter
    {
        public object ConvertFrom(string value) => 
            new CustomType { SomeValue = value.Length };
    }
}
