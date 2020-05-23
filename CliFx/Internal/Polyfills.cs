// ReSharper disable CheckNamespace

// Polyfills to bridge the missing APIs in older versions of the framework/standard.

#if NETSTANDARD2_0
namespace System
{
    using Linq;

    internal static class Extensions
    {
        public static bool Contains(this string str, char c) =>
            str.Any(i => i == c);

        public static bool StartsWith(this string str, char c) =>
            str.Length > 0 && str[0] == c;

        public static bool EndsWith(this string str, char c) =>
            str.Length > 0 && str[str.Length - 1] == c;
    }
}

namespace System.Collections.Generic
{
    internal static class Extensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key, out var result) ? result! : default!;
    }
}

namespace System.Linq
{
    using Collections.Generic;

    internal static class Extensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer) =>
            new HashSet<T>(source, comparer);
    }
}
#endif