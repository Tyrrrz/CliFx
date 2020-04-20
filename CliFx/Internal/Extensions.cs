using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CliFx.Internal
{
    internal static class Extensions
    {
        public static string Repeat(this char c, int count) => new string(c, count);

        public static string AsString(this char c) => c.Repeat(1);

        public static StringBuilder AppendIfNotEmpty(this StringBuilder builder, char value) =>
            builder.Length > 0 ? builder.Append(value) : builder;

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
    }
}