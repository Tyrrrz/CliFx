using System;
using System.Collections.Generic;

namespace CliFx.Internal
{
    internal static class CollectionExtensions
    {
        public static int IndexOf<T>(this IReadOnlyList<T> source, Func<T, bool> predicate, int startIndex = 0)
        {
            for (var i = startIndex; i < source.Count; i++)
            {
                if (predicate(source[i]))
                    return i;
            }

            return -1;
        }
    }
}