using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CliFx.Utils.Extensions;

internal static class TypeExtensions
{
    extension(Type type)
    {
        public bool Implements(Type interfaceType) => type.GetInterfaces().Contains(interfaceType);

        public Type? TryGetNullableUnderlyingType() => Nullable.GetUnderlyingType(type);

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

        public MethodInfo? TryGetStaticParseMethod(bool withFormatProvider = false)
        {
            var argumentTypes = withFormatProvider
                ? new[] { typeof(string), typeof(IFormatProvider) }
                : new[] { typeof(string) };

            return type.GetMethod(
                "Parse",
                BindingFlags.Public | BindingFlags.Static,
                null,
                argumentTypes,
                null
            );
        }

        public bool IsToStringOverriden()
        {
            var toStringMethod = type.GetMethod(nameof(ToString), Type.EmptyTypes);
            return toStringMethod?.GetBaseDefinition()?.DeclaringType
                != toStringMethod?.DeclaringType;
        }
    }
}
