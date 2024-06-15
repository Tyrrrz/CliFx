using System;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Schema;

/// <summary>
/// Describes a CLR property.
/// </summary>
public class PropertyBinding(
    Type propertyType,
    Func<object, object?> getValue,
    Action<object, object?> setValue
    )
{
    /// <summary>
    /// Underlying property type.
    /// </summary>
    public Type PropertyType { get; } = propertyType;
    
    /// <summary>
    /// Gets the current value of the property on the specified instance.
    /// </summary>
    public object? GetValue(object instance) => getValue(instance);

    /// <summary>
    /// Sets the value of the property on the specified instance.
    /// </summary>
    public void SetValue(object instance, object? value) => setValue(instance, value);
}

internal static class PropertyBindingExtensions
{
    public static IReadOnlyList<object?>? TryGetValidValues(this PropertyBinding binding) =>
        binding.PropertyType.IsEnum
            ? binding.PropertyType.GetEnumValuesAsUnderlyingType().Cast<object?>().ToArray()
            : null;
}