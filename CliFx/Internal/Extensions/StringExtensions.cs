using System;
using System.Collections.Generic;
using System.Text;

namespace CliFx.Internal.Extensions
{
    internal static class StringExtensions
    {
        public static string Repeat(this char c, int count)
        {
            return new string(c, count);
        }

        public static string AsString(this char c)
        {
            return c.Repeat(1);
        }

        public static string Quote(this string str)
        {
            return $"\"{str}\"";
        }

        public static string JoinToString<T>(this IEnumerable<T> source, string separator)
        {
            return string.Join(separator, source);
        }

        public static StringBuilder AppendIfNotEmpty(this StringBuilder builder, char value)
        {
            return builder.Length > 0 ? builder.Append(value) : builder;
        }

        public static string ToFormattableString(this object obj,
                                                 IFormatProvider? formatProvider = null,
                                                 string? format = null)
        {
            return obj is IFormattable formattable ? formattable.ToString(format, formatProvider) : obj.ToString();
        }
    }
}