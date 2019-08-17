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

        public static StringBuilder Trim(this StringBuilder builder)
        {
            while (builder.Length > 0 && char.IsWhiteSpace(builder[0]))
                builder.Remove(0, 1);

            while (builder.Length > 0 && char.IsWhiteSpace(builder[builder.Length - 1]))
                builder.Remove(builder.Length - 1, 1);

            return builder;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key, out var result) ? result : default;

        public static IEnumerable<T> ExceptNull<T>(this IEnumerable<T> source) where T : class => source.Where(i => i != null);

        public static bool Implements(this Type type, Type interfaceType) => type.GetInterfaces().Contains(interfaceType);

        public static Type GetEnumerableUnderlyingType(this Type type)
        {
            if (type == typeof(IEnumerable))
                return typeof(object);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments().FirstOrDefault();

            return type.GetInterfaces()
                .Select(GetEnumerableUnderlyingType)
                .ExceptNull()
                .OrderByDescending(t => t != typeof(object))
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