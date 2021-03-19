using System;
using System.Collections.Generic;

namespace CliFx.Utils.Extensions
{
    internal static class StringExtensions
    {
        public static string? NullIfWhiteSpace(this string str) =>
            !string.IsNullOrWhiteSpace(str)
                ? str
                : null;

        public static string Repeat(this char c, int count) => new(c, count);

        public static string AsString(this char c) => c.Repeat(1);

        public static string Quote(this string str) => $"\"{str}\"";

        public static string JoinToString<T>(this IEnumerable<T> source, string separator) =>
            string.Join(separator, source);

        public static string ToString(
            this object obj,
            IFormatProvider? formatProvider = null,
            string? format = null) =>
            obj is IFormattable formattable
                ? formattable.ToString(format, formatProvider)
                : obj.ToString();
    }
}