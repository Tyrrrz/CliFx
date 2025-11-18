using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Utils.Extensions;

internal static class CollectionExtensions
{
    extension<T>(IEnumerable<T?> source)
        where T : class
    {
        public IEnumerable<T> WhereNotNull()
        {
            foreach (var i in source)
            {
                if (i is not null)
                    yield return i;
            }
        }

        public Array ToNonGenericArray(Type elementType)
        {
            var sourceAsCollection = source as ICollection ?? source.ToArray();

            var array = Array.CreateInstance(elementType, sourceAsCollection.Count);
            sourceAsCollection.CopyTo(array, 0);

            return array;
        }
    }

    extension(IEnumerable<string?> source)
    {
        public IEnumerable<string> WhereNotNullOrWhiteSpace()
        {
            foreach (var i in source)
            {
                if (!string.IsNullOrWhiteSpace(i))
                    yield return i;
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
