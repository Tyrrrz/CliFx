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

        public static string SubstringUntil(this string s, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? s : s.Substring(0, index);
        }

        public static string SubstringAfter(this string s, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            var index = s.IndexOf(sub, comparison);
            return index < 0 ? string.Empty : s.Substring(index + sub.Length, s.Length - index - sub.Length);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key, out var result) ? result : default;

        public static string TrimStart(this string s, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            while (s.StartsWith(sub, comparison))
                s = s.Substring(sub.Length);

            return s;
        }

        public static string TrimEnd(this string s, string sub, StringComparison comparison = StringComparison.Ordinal)
        {
            while (s.EndsWith(sub, comparison))
                s = s.Substring(0, s.Length - sub.Length);

            return s;
        }

        public static string JoinToString<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);

        public static bool IsDerivedFrom(this Type type, Type baseType)
        {
            for (var currentType = type; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType == baseType)
                    return true;
            }

            return false;
        }

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
    }
}