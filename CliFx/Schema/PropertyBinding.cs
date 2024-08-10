using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CliFx.Schema;

/// <summary>
/// Represents a wrapper around a CLR property that provides read and write access to its value.
/// </summary>
public class PropertyBinding(
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicMethods
    )]
        Type type,
    Func<object, object?> get,
    Action<object, object?> set
)
{
    /// <summary>
    /// Underlying CLR type of the property.
    /// </summary>
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicMethods
    )]
    public Type Type { get; } = type;

    /// <summary>
    /// Gets the current value of the property on the specified instance.
    /// </summary>
    public object? Get(object instance) => get(instance);

    /// <summary>
    /// Sets the current value of the property on the specified instance.
    /// </summary>
    public void Set(object instance, object? value) => set(instance, value);

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

// Generic version of the type is used to simplify initialization from the source-generated code
// and to enforce static references to all the types used in the binding.
// The non-generic version is used internally by the framework when operating in a dynamic context.
/// <inheritdoc cref="PropertyBinding" />
public class PropertyBinding<
    TObject,
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicMethods
    )]
        TProperty
>(Func<TObject, TProperty?> get, Action<TObject, TProperty?> set)
    : PropertyBinding(
        typeof(TProperty),
        o => get((TObject)o),
        (o, v) => set((TObject)o, (TProperty?)v)
    );
