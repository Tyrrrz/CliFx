using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Utils.Extensions
{
    internal static class CollectionExtensions
    {
        public static void RemoveRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            foreach (var item in items)
                source.Remove(item);
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IDictionary dictionary,
            IEqualityComparer<TKey> comparer) =>
            dictionary
                .Cast<DictionaryEntry>()
                .ToDictionary(entry => (TKey) entry.Key, entry => (TValue) entry.Value, comparer)!;
    }
}