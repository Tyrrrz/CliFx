using System;
using System.Collections.Generic;

namespace CliFx.Internal
{
    internal static class Extensions
    {
        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);

        public static string SubstringUntil(this string s, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? s : s.Substring(0, index);
        }

        public static string SubstringAfter(this string s, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? string.Empty : s.Substring(index + sub.Length, s.Length - index - sub.Length);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key, out var result) ? result : default;

        public static string TrimStart(this string s, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            while (s.StartsWith(sub, comparison))
                s = s.Substring(sub.Length);

            return s;
        }

        public static string TrimEnd(this string s, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            while (s.EndsWith(sub, comparison))
                s = s.Substring(0, s.Length - sub.Length);

            return s;
        }

        public static string JoinToString<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);

        public static bool IsDerivedFrom(this Type type, Type baseType)
        {
            for (var currentType = type; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType == baseType)
                    return true;
            }

            return false;
        }
    }
}