using System.Text.Json;
using Newtonsoft.Json;

namespace CliFx.Tests.Utils.Extensions
{
    internal static class JsonExtensions
    {
        public static T DeserializeJson<T>(this string json) => JsonConvert.DeserializeObject<T>(json);

        public static JsonElement ParseJson(this string json)
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
    }
}