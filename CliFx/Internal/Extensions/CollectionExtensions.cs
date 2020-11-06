using System.Collections.Generic;
using System.Linq;

namespace CliFx.Internal.Extensions
{
    internal static class CollectionExtensions
    {
        public static void RemoveRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            foreach (var item in items)
                source.Remove(item);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) =>
            !source?.Any() ?? true;

        public static bool NotEmpty<T>(this IEnumerable<T>? source) =>
            !source.IsNullOrEmpty();
    }
}