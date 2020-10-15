﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CliFx.Internal.Extensions
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

            return type
                .GetInterfaces()
                .Select(GetEnumerableUnderlyingType)
                .Where(t => t != null)
                .OrderByDescending(t => t != typeof(object)) // prioritize more specific types
                .FirstOrDefault();
        }

        public static MethodInfo GetToStringMethod(this Type type) => type.GetMethod(nameof(ToString), Type.EmptyTypes);

        public static bool IsToStringOverriden(this Type type) => type.GetToStringMethod() != typeof(object).GetToStringMethod();

        public static MethodInfo GetStaticParseMethod(this Type type, bool withFormatProvider = false)
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

        public static T InstanceOf<T>(this Type type) =>
            type.Implements(typeof(T))
                ? (T) Activator.CreateInstance(type)
                : throw new ArgumentException();
    }
}