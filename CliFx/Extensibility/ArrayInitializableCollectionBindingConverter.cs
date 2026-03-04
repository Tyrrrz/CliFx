using System;
using System.Collections.Generic;

namespace CliFx.Extensibility;

/// <summary>
/// Collection converter that first converts raw values into a <typeparamref name="TElement" /> array
/// (via an <see cref="ArrayCollectionBindingConverter{TElement}" /> created internally) and then produces
/// the final <typeparamref name="TCollection" /> using a caller-supplied factory delegate.
/// Suitable for array-initializable concrete collection types such as <see cref="System.Collections.Generic.List{T}" />.
/// </summary>
public class ArrayInitializableCollectionBindingConverter<TElement, TCollection>
    : CollectionBindingConverter<TCollection>
{
    private readonly ArrayCollectionBindingConverter<TElement> _arrayConverter;
    private readonly Func<TElement[], TCollection> _collectionFactory;

    /// <summary>
    /// Initializes a new instance with an optional per-element converter and a collection factory.
    /// </summary>
    /// <param name="elementConverter">Per-element converter; <see langword="null" /> means string pass-through.</param>
    /// <param name="collectionFactory">Factory that constructs the target collection from the element array.</param>
    public ArrayInitializableCollectionBindingConverter(
        BindingConverter<TElement>? elementConverter,
        Func<TElement[], TCollection> collectionFactory
    )
    {
        _arrayConverter = new ArrayCollectionBindingConverter<TElement>(elementConverter);
        _collectionFactory = collectionFactory;
    }

    /// <inheritdoc />
    public override TCollection ConvertMany(IReadOnlyList<string?> rawValues)
    {
        var array = _arrayConverter.ConvertMany(rawValues);
        return _collectionFactory(array);
    }
}
