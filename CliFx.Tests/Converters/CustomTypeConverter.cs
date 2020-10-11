using CliFx.Domain;
using Newtonsoft.Json;

namespace CliFx.Tests.Converters
{
    [JsonObject]
    public class CustomType
    {
        [JsonProperty]
        public int Length { get; set; }
    }

    internal class CustomTypeConverter : StringTypeConverter<CustomType>
    {
        public override CustomType Convert(string input) => new CustomType() { Length = input.Length };
    }
}
