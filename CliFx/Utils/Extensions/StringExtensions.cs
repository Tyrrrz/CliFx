using System;

namespace CliFx.Utils.Extensions;

internal static class StringExtensions
{
    extension(string str)
    {
        public string? NullIfWhiteSpace() => !string.IsNullOrWhiteSpace(str) ? str : null;
    }

    extension(char c)
    {
        public string Repeat(int count) => new(c, count);

        public string AsString() => c.Repeat(1);
    }

    extension(object obj)
    {
        public string? ToString(IFormatProvider? formatProvider = null, string? format = null) =>
            obj is IFormattable formattable
                ? formattable.ToString(format, formatProvider)
                : obj.ToString();
    }
}
