using System;
using System.Collections.Generic;
using System.Text;

namespace CliFx.Internal
{
    internal static class StringExtensions
    {
        public static string Repeat(this char c, int count) => new string(c, count);

        public static string AsString(this char c) => c.Repeat(1);

        public static string Quote(this string str) => $"\"{str}\"";

        public static string QuoteIfContainsWhiteSpace(this string str) =>
            str.Contains(' ')
                ? str.Quote()
                : str;

        public static string JoinToString<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);

        public static StringBuilder AppendIfNotEmpty(this StringBuilder builder, char value) =>
            builder.Length > 0 ? builder.Append(value) : builder;

        public static string ToFormattableString(this object obj,
            IFormatProvider? formatProvider = null, string? format = null) =>
            obj is IFormattable formattable
                ? formattable.ToString(format, formatProvider)
                : obj.ToString();
    }
}