using System.Text;

namespace CliFx.Internal
{
    internal static class StringExtensions
    {
        public static string Repeat(this char c, int count) => new string(c, count);

        public static string AsString(this char c) => c.Repeat(1);

        public static StringBuilder AppendIfNotEmpty(this StringBuilder builder, char value) =>
            builder.Length > 0 ? builder.Append(value) : builder;

        public static bool IsEmptyOrWhiteSpace(this string s) => s is object && string.IsNullOrWhiteSpace(s);
    }
}