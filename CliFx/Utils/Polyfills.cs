// ReSharper disable CheckNamespace

#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.IO;

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
}

internal static partial class PolyfillExtensions
{
    public static void Write(this Stream stream, byte[] buffer) =>
        stream.Write(buffer, 0, buffer.Length);
}


namespace System.Linq
{
    internal static class PolyfillExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer) =>
            new(source, comparer);
    }
}

namespace System.Collections.Generic
{
    internal static class PolyfillExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key!, out var result) ? result! : default!;
    }
}
#endif