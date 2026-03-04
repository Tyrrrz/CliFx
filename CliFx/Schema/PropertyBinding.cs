using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

/// <summary>
/// Provides read and write access to a CLR property.
/// </summary>
public class PropertyBinding(
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

    [RequiresUnreferencedCode(
        "Uses Type.GetInterfaces() which may not be available after trimming."
    )]
    private static Type GetEffectiveEnumType(Type type)
    {
        var enumerableUnderlyingType = type.TryGetEnumerableUnderlyingType();
        if (enumerableUnderlyingType is not null)
            return GetEffectiveEnumType(enumerableUnderlyingType);

        var nullableUnderlyingType = type.TryGetNullableUnderlyingType();
        if (nullableUnderlyingType is not null)
            return GetEffectiveEnumType(nullableUnderlyingType);

        return type;
    }

    [RequiresUnreferencedCode(
        "Uses reflection to discover valid enum values. Not compatible with trimming."
    )]
    internal IReadOnlyList<object?>? TryGetValidValues()
    {
        var effectiveType = GetEffectiveEnumType(Type);
        if (effectiveType.IsEnum)
            return Enum.GetNames(effectiveType);

        return null;
    }
}

/// <inheritdoc cref="PropertyBinding" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public class PropertyBinding<TObject, TProperty>(
    string name,
    Func<TObject, TProperty?> getValue,
    Action<TObject, TProperty?> setValue
)
    : PropertyBinding(
        name,
        typeof(TProperty),
        o => getValue((TObject)o),
        (o, v) => setValue((TObject)o, (TProperty?)v)
    );
