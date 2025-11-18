using System;

namespace CliFx.Analyzers.Utils.Extensions;

internal static class StringExtensions
{
    extension(string str)
    {
        public string TrimEnd(string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            while (str.EndsWith(sub, comparison))
                str = str[..^sub.Length];

            return str;
        }
    }
}
