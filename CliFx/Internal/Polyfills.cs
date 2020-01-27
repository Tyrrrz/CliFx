#if NET45 || NETSTANDARD2_0
using System.Collections.Generic;
using System.Text;

namespace CliFx.Internal
{
    internal static class Polyfills
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> self, TKey key) =>
            self.TryGetValue(key, out var value) ? value : default;

        public static StringBuilder AppendJoin<T>(this StringBuilder self, string separator, IEnumerable<T> items) =>
            self.Append(string.Join(separator, items));
    }
}
#endif