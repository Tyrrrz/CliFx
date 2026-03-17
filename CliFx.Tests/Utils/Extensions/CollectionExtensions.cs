using System.Collections.Generic;

namespace CliFx.Tests.Utils.Extensions;

internal static class CollectionExtensions
{
    extension<T>(IEnumerable<T?> source)
    {
        public IEnumerable<T> WhereNotNull()
        {
            foreach (var i in source)
            {
                if (i is not null)
                    yield return i;
            }
        }
    }
}
