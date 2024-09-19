using System;
using System.Collections.Generic;

namespace CliFx.SourceGeneration.Utils.Extensions;

internal static class StringExtensions
{
    public static string SubstringUntilLast(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal
    )
    {
        var index = str.LastIndexOf(sub, comparison);
        return index < 0 ? str : str[..index];
    }

    public static string SubstringAfterLast(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal
    )
    {
        var index = str.LastIndexOf(sub, comparison);
        return index >= 0 ? str.Substring(index + sub.Length, str.Length - index - sub.Length) : "";
    }

    public static string JoinToString<T>(this IEnumerable<T> source, string separator) =>
        string.Join(separator, source);
}
