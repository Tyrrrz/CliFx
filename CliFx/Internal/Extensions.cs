using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Internal
{
    internal static class Extensions
    {
        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);

        public static string AsString(this char c) => new string(c, 1);

        public static string JoinToString<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key, out var result) ? result : default;

        public static IEnumerable<T> ExceptNull<T>(this IEnumerable<T> source) where T : class => source.Where(i => i != null);

        public static bool IsEnumerable(this Type type) =>
            type == typeof(IEnumerable) || type.GetInterfaces().Contains(typeof(IEnumerable));

        public static IReadOnlyList<Type> GetIEnumerableUnderlyingTypes(this Type type)
        {
            if (type == typeof(IEnumerable))
                return new[] {typeof(object)};

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return new[] {type.GetGenericArguments()[0]};

            return type.GetInterfaces()
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(t => t.GetGenericArguments()[0])
                .ToArray();
        }

        public static Array ToNonGenericArray(this ICollection source, Type elementType)
        {
            var array = Array.CreateInstance(elementType, source.Count);
            source.CopyTo(array, 0);

            return array;
        }

        public static bool Implements(this Type type, Type interfaceType) => type.GetInterfaces().Contains(interfaceType);
    }
}