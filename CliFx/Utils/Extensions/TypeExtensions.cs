using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CliFx.Utils.Extensions
{
    internal static class TypeExtensions
    {
        public static bool Implements(this Type type, Type interfaceType) =>
            type.GetInterfaces().Contains(interfaceType);

        public static Type? TryGetNullableUnderlyingType(this Type type) => Nullable.GetUnderlyingType(type);

        public static Type? TryGetEnumerableUnderlyingType(this Type type)
        {
            if (type.IsPrimitive)
                return null;

            if (type == typeof(IEnumerable))
                return typeof(object);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments().FirstOrDefault();

            return type
                .GetInterfaces()
                .Select(TryGetEnumerableUnderlyingType)
                .Where(t => t is not null)
                .OrderByDescending(t => t != typeof(object)) // prioritize more specific types
                .FirstOrDefault();
        }

        public static MethodInfo? TryGetStaticParseMethod(this Type type, bool withFormatProvider = false)
        {
            var argumentTypes = withFormatProvider
                ? new[] {typeof(string), typeof(IFormatProvider)}
                : new[] {typeof(string)};

            return type.GetMethod("Parse",
                BindingFlags.Public | BindingFlags.Static,
                null, argumentTypes, null
            );
        }

        public static Array ToNonGenericArray<T>(this IEnumerable<T> source, Type elementType)
        {
            var sourceAsCollection = source as ICollection ?? source.ToArray();

            var array = Array.CreateInstance(elementType, sourceAsCollection.Count);
            sourceAsCollection.CopyTo(array, 0);

            return array;
        }

        public static bool IsToStringOverriden(this Type type)
        {
            var toStringMethod = type.GetMethod(nameof(ToString), Type.EmptyTypes);
            return toStringMethod?.GetBaseDefinition()?.DeclaringType != toStringMethod?.DeclaringType;
        }

        // Types supported by `Convert.ChangeType(...)`
        private static readonly HashSet<Type> ConvertibleTypes = new()
        {
            typeof(bool),
            typeof(char),
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
            typeof(string),
            typeof(object)
        };

        public static bool IsConvertible(this Type type) => ConvertibleTypes.Contains(type);
    }
}