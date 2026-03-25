using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Utils.Extensions;

internal static class CollectionExtensions
{
    extension<T>(IEnumerable<T?> source)
    {
        public IEnumerable<T> WhereNotNull()
        {
            foreach (var item in source)
            {
                if (item is not null)
                    yield return item;
            }
        }
    }

    extension(IEnumerable<string?> source)
    {
        public IEnumerable<string> WhereNotNullOrWhiteSpace()
        {
            foreach (var item in source)
            {
                if (!string.IsNullOrWhiteSpace(item))
                    yield return item;
            }
        }
    }

    extension<T>(ICollection<T> source)
    {
        public void RemoveRange(IEnumerable<T> items)
        {
            foreach (var item in items)
                source.Remove(item);
        }
    }

    extension(IDictionary dictionary)
    {
        public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(IEqualityComparer<TKey> comparer)
            where TKey : notnull =>
            dictionary
                .Cast<DictionaryEntry>()
                .ToDictionary(entry => (TKey)entry.Key, entry => (TValue)entry.Value!, comparer);
    }
}
