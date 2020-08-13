namespace CliFx.Internal.Extensions
{
    using System.Collections.Generic;

    internal static class CollectionExtensions
    {
        public static void RemoveRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            foreach (T item in items)
                source.Remove(item);
        }
    }
}