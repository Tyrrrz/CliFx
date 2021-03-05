using Newtonsoft.Json;

namespace CliFx.Tests.Utils
{
    internal static class JsonExtensions
    {
        public static T DeserializeJson<T>(this string json) => JsonConvert.DeserializeObject<T>(json);
    }
}