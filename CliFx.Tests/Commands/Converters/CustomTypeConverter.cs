namespace CliFx.Tests.Commands.Converters
{
    public class CustomTypeConverter : IArgumentValueConverter
    {
        public object ConvertFrom(string value) => 
            new CustomType { SomeValue = value.Length };
    }
}
