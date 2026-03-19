using System;
using System.Collections.Generic;

namespace CliFx.Binding;

/// <summary>
/// Provides read and write access to a CLR property.
/// </summary>
public class PropertyDescriptor(
    string name,
    Type type,
    Func<object, object?> getValue,
    Action<object, object?> setValue
)
{
    /// <summary>
    /// Property name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Underlying CLR type of the property.
    /// </summary>
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
            return Enum.GetNames(Type);
        }

        if (
            Nullable.GetUnderlyingType(Type) is Type nullableUnderlyingType
            && nullableUnderlyingType.IsEnum
        )
        {
            return Enum.GetNames(nullableUnderlyingType);
        }

        return null;
    }
}

/// <inheritdoc cref="PropertyDescriptor" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public class PropertyDescriptor<TObject, TProperty>(
    string name,
    Func<TObject, TProperty?> getValue,
    Action<TObject, TProperty?> setValue
)
    : PropertyDescriptor(
        name,
        typeof(TProperty),
        o => getValue((TObject)o),
        (o, v) => setValue((TObject)o, (TProperty?)v)
    );
