using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Internal
{
    internal static class TypeExtensions
    {
        public static bool Implements(this Type type, Type interfaceType) => type.GetInterfaces().Contains(interfaceType);

        public static Type? GetNullableUnderlyingType(this Type type) => Nullable.GetUnderlyingType(type);

        public static Type? GetEnumerableUnderlyingType(this Type type)
        {
            if (type.IsPrimitive)
                return null;

            if (type == typeof(IEnumerable))
                return typeof(object);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments().FirstOrDefault();

            return type.GetInterfaces()
                .Select(GetEnumerableUnderlyingType)
                .Where(t => t != null)
                .OrderByDescending(t => t != typeof(object)) // prioritize more specific types
                .FirstOrDefault();
        }

        public static Array ToNonGenericArray<T>(this IEnumerable<T> source, Type elementType)
        {
            var sourceAsCollection = source as ICollection ?? source.ToArray();

            var array = Array.CreateInstance(elementType, sourceAsCollection.Count);
            sourceAsCollection.CopyTo(array, 0);

            return array;
        }

        public static bool OverridesToStringMethod(this object obj) => obj?.ToString() != obj?.GetType().ToString();

        public static bool IsEnumerable(this object? obj) => obj?.GetType().GetEnumerableUnderlyingType() is object;
    }
}