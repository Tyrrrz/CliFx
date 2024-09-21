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
    /// Sets the current value of the property on the specified instance.
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
                // that someone will be AOT-compiling a net6.0 or older app anyway.
                Type.GetEnumValues();
#endif

            return values.Cast<object?>().ToArray();
        }

        return null;
    }
}

/// <inheritdoc cref="PropertyBinding" />
/// <remarks>
/// Generic version of the type is used to simplify initialization from source-generated code and
/// to enforce static references to all types used in the binding.
/// The non-generic version is used internally by the framework when operating in a dynamic context.
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
