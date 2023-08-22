using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Utils.Extensions;

internal static class CollectionExtensions
{
    public static IEnumerable<(T value, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        var i = 0;
        foreach (var o in source)
            yield return (o, i++);
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        foreach (var i in source)
        {
            if (i is not null)
                yield return i;
        }
    }

    public static IEnumerable<string> WhereNotNullOrWhiteSpace(this IEnumerable<string?> source)
    {
        foreach (var i in source)
        {
            if (!string.IsNullOrWhiteSpace(i))
                yield return i;
        }
    }

    public static void RemoveRange<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        foreach (var item in items)
            source.Remove(item);
    }

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
        this IDictionary dictionary,
        IEqualityComparer<TKey> comparer
    ) =>
        dictionary
            .Cast<DictionaryEntry>()
            .ToDictionary(entry => (TKey)entry.Key, entry => (TValue)entry.Value, comparer);

    public static Array ToNonGenericArray<T>(this IEnumerable<T> source, Type elementType)
    {
        var sourceAsCollection = source as ICollection ?? source.ToArray();

        var array = Array.CreateInstance(elementType, sourceAsCollection.Count);
        sourceAsCollection.CopyTo(array, 0);

        return array;
    }
}
