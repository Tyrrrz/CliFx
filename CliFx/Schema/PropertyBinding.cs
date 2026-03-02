using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

    internal IReadOnlyList<object?>? TryGetValidValues()
    {
        if (Type.IsEnum)
        {
#if NET7_0_OR_GREATER
            return Type.GetEnumValuesAsUnderlyingType().Cast<object?>().ToArray();
#else
            return Type.GetEnumValues().Cast<object?>().ToArray();
#endif
        }

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
