// ReSharper disable CheckNamespace
// ReSharper disable RedundantUsingDirective

// Polyfills to bridge the missing APIs in older versions of the framework/standard.

using System;
using System.Collections.Generic;

#if NETSTANDARD2_0
internal static partial class PolyfillExtensions
{
    public static bool StartsWith(this string str, char c) =>
        str.Length > 0 && str[0] == c;

    public static bool EndsWith(this string str, char c) =>
        str.Length > 0 && str[str.Length - 1] == c;

    public static string[] Split(this string str, char separator, StringSplitOptions splitOptions) =>
        str.Split(new[] {separator}, splitOptions);
}

internal static partial class PolyfillExtensions
{
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }

    public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
        dic.TryGetValue(key!, out var result) ? result! : default!;
}

namespace System.Linq
{
    internal static class PolyfillExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer) =>
            new(source, comparer);
    }
}
#endif