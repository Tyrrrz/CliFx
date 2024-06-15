using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CliFx.Schema;

/// <summary>
/// Represents a CLR property binding.
/// </summary>
public class PropertyBinding(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicMethods)]
    Type type,
    Func<object, object?> getValue,
    Action<object, object?> setValue
)
{
    /// <summary>
    /// Underlying CLR type of the property.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicMethods)]
    public Type Type { get; } = type;

    /// <summary>
    /// Gets the current value of the property on the specified instance.
    /// </summary>
    public object? GetValue(object instance) => getValue(instance);

    /// <summary>
    /// Sets the value of the property on the specified instance.
    /// </summary>
    public void SetValue(object instance, object? value) => setValue(instance, value);

    internal IReadOnlyList<object?>? TryGetValidValues()
    {
        if (Type.IsEnum)
        {
            var values =
#if NET7_0_OR_GREATER
            Type.GetEnumValuesAsUnderlyingType();
#else
                // AOT-compatible APIs are not available here, but it's unlikely
                // someone will be AOT-compiling a net6.0 or older app anyway.
                Type.GetEnumValues();
#endif

            return values.Cast<object?>().ToArray();
        }

        return null;
    }
}
