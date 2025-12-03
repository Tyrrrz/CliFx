using System;
using System.Collections.Generic;

namespace CliFx.SourceGeneration.Utils.Extensions;

internal static class StringExtensions
{
    extension(string str)
    {
        public string SubstringUntilLast(
            string sub,
            StringComparison comparison = StringComparison.Ordinal
        )
        {
            var index = str.LastIndexOf(sub, comparison);
            return index < 0 ? str : str[..index];
        }

        public string SubstringAfterLast(
            string sub,
            StringComparison comparison = StringComparison.Ordinal
        )
        {
            var index = str.LastIndexOf(sub, comparison);
            return index >= 0
                ? str.Substring(index + sub.Length, str.Length - index - sub.Length)
                : "";
        }
    }

    extension<T>(IEnumerable<T> source)
    {
        public string JoinToString(string separator) => string.Join(separator, source);
    }
}
