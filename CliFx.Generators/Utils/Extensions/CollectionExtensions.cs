using System.Collections.Generic;

namespace CliFx.Generators.Utils.Extensions;

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
}
