using System;
using System.Linq;
using System.Reflection;

namespace CliFx.Utils.Extensions;

internal static class PropertyExtensions
{
    public static bool IsRequired(this PropertyInfo propertyInfo) =>
        // Match attribute by name to avoid depending on .NET 7.0+ and to allow polyfilling
        propertyInfo.GetCustomAttributes().Any(a =>
            string.Equals(
                a.GetType().FullName,
                "System.Runtime.CompilerServices.RequiredMemberAttribute",
                StringComparison.Ordinal
            )
        );
}