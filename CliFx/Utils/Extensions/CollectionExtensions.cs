using System.Collections.Generic;

namespace CliFx.Utils.Extensions
{
    internal static class CollectionExtensions
    {
        public static void RemoveRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            foreach (var item in items)
                source.Remove(item);
        }
    }
}