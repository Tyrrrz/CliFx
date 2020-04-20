using System;

namespace CliFx.Analyzers.Internal
{
    internal static class StringExtensions
    {
        public static string SubstringUntil(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? s : s.Substring(0, index);
        }

        public static string SubstringUntilLast(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.LastIndexOf(sub, comparison);
            return index < 0 ? s : s.Substring(0, index);
        }

        public static string SubstringAfter(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? string.Empty : s.Substring(index + sub.Length, s.Length - index - sub.Length);
        }

        public static string SubstringAfterLast(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.LastIndexOf(sub, comparison);
            return index < 0 ? string.Empty : s.Substring(index + sub.Length, s.Length - index - sub.Length);
        }
    }
}