using System;
using System.Diagnostics.CodeAnalysis;

namespace CliFx.Schema;

/// <summary>
/// Describes a CLR property.
/// </summary>
public class PropertyDescriptor(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
    Type type,
    Func<object, object?> getValue,
    Action<object, object?> setValue
)
{
    /// <summary>
    /// Property's CLR type.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
    public Type Type { get; } = type;

    /// <summary>
    /// Gets the current value of the property on the specified instance.
    /// </summary>
    public object? GetValue(object instance) => getValue(instance);

    /// <summary>
    /// Sets the value of the property on the specified instance.
    /// </summary>
    public void SetValue(object instance, object? value) => setValue(instance, value);
}
