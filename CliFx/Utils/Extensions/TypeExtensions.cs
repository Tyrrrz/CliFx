using System;
using System.Diagnostics.CodeAnalysis;

namespace CliFx.Utils.Extensions;

internal static class TypeExtensions
{
    public static bool IsToStringOverriden(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] this Type type
    )
    {
        var toStringMethod = type.GetMethod(nameof(ToString), []);
        return toStringMethod?.GetBaseDefinition().DeclaringType != toStringMethod?.DeclaringType;
    }
}
