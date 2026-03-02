using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

/// <summary>
/// Provides read and write access to a CLR property.
/// </summary>
public class PropertyBinding(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type,
    Func<object, object?> getValue,
    Action<object, object?> setValue
)
{
    /// <summary>
    /// Underlying CLR type of the property.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
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
public class PropertyBinding<
    TObject,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TProperty
>(Func<TObject, TProperty?> getValue, Action<TObject, TProperty?> setValue)
    : PropertyBinding(
        typeof(TProperty),
        o => getValue((TObject)o),
        (o, v) => setValue((TObject)o, (TProperty?)v)
    );
