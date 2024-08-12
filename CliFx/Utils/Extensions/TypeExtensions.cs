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
    ) =>
        type.GetInterfaces()
            .Select(i =>
            {
                if (i == typeof(IEnumerable))
                    return typeof(object);

                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return i.GetGenericArguments().FirstOrDefault();

                return null;
            })
            .WhereNotNull()
            // Every IEnumerable<T> implements IEnumerable (which is essentially IEnumerable<object>),
            // so we try to get a more specific underlying type. Still, if the type only implements
            // IEnumerable<object> and nothing else, then we'll just return that.
            .MaxBy(t => t != typeof(object));

    public static bool IsToStringOverriden(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] this Type type
    )
    {
        var toStringMethod = type.GetMethod(nameof(ToString), []);
        return toStringMethod?.GetBaseDefinition().DeclaringType != toStringMethod?.DeclaringType;
    }
}
