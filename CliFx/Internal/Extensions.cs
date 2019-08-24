using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CliFx.Internal
{
    internal static class Extensions
    {
        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);

        public static string Repeat(this char c, int count) => new string(c, count);

        public static string AsString(this char c) => c.Repeat(1);

        public static string JoinToString<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);

        public static string SubstringUntilLast(this string s, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.LastIndexOf(sub, comparison);
            return index < 0 ? s : s.Substring(0, index);
        }

        public static StringBuilder AppendIfNotEmpty(this StringBuilder builder, char value) =>
            builder.Length > 0 ? builder.Append(value) : builder;

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T value)
        {
            foreach (var i in source)
                yield return i;

            yield return value;
        }

        public static bool Implements(this Type type, Type interfaceType) => type.GetInterfaces().Contains(interfaceType);

        public static Type GetEnumerableUnderlyingType(this Type type)
        {
            if (type == typeof(IEnumerable))
                return typeof(object);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments().FirstOrDefault();

            return type.GetInterfaces()
                .Select(GetEnumerableUnderlyingType)
                .Where(t => t != default)
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