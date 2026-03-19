using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CliFx.Utils.Extensions;

internal static class TypeExtensions
{
    extension(Type type)
    {
        public Type? TryGetNullableUnderlyingType() => Nullable.GetUnderlyingType(type);

        [RequiresUnreferencedCode(
            "Uses Type.GetInterfaces() which may not be available after trimming."
        )]
        public Type? TryGetEnumerableUnderlyingType()
        {
            if (type.IsPrimitive)
                return null;

            if (type == typeof(IEnumerable))
                return typeof(object);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments().FirstOrDefault();

            return type.GetInterfaces()
                .Select(TryGetEnumerableUnderlyingType)
                .Where(t => t is not null)
                // Every IEnumerable<T> implements IEnumerable (which is essentially IEnumerable<object>),
                // so we try to get a more specific underlying type. Still, if the type only implements
                // IEnumerable<object> and nothing else, then we'll just return that.
                .MaxBy(t => t != typeof(object));
        }
    }
}
