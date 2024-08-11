using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CliFx.Utils.Extensions;

internal static class TypeExtensions
{
    public static Type? TryGetEnumerableUnderlyingType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type type
    )
    {
        if (type.IsPrimitive)
            return null;

        if (type == typeof(IEnumerable))
            return typeof(object);

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments().FirstOrDefault();

        return type.GetInterfaces()
            .Select(t => TryGetEnumerableUnderlyingType(t))
            .Where(t => t is not null)
            // Every IEnumerable<T> implements IEnumerable (which is essentially IEnumerable<object>),
            // so we try to get a more specific underlying type. Still, if the type only implements
            // IEnumerable<object> and nothing else, then we'll just return that.
            .MaxBy(t => t != typeof(object));
    }

    public static bool IsToStringOverriden(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] this Type type
    )
    {
        var toStringMethod = type.GetMethod(nameof(ToString), Type.EmptyTypes);
        return toStringMethod?.GetBaseDefinition().DeclaringType != toStringMethod?.DeclaringType;
    }
}
