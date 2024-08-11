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
    )
        where TKey : notnull =>
        dictionary
            .Cast<DictionaryEntry>()
            .ToDictionary(entry => (TKey)entry.Key, entry => (TValue)entry.Value!, comparer);
}
