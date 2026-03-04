using System;
using System.Collections.Generic;

namespace CliFx.Extensibility;

/// <summary>
/// Collection converter that first converts raw values into a <typeparamref name="TElement" /> array
/// (via an <see cref="ArraySequenceBindingConverter{TElement}" /> created internally) and then produces
/// the final <typeparamref name="TCollection" /> using a caller-supplied factory delegate.
/// Suitable for array-initializable concrete collection types such as <see cref="System.Collections.Generic.List{T}" />.
/// </summary>
public class ArrayInitializableSequenceBindingConverter<TElement, TCollection>
    : SequenceBindingConverter<TCollection>
{
    private readonly ArraySequenceBindingConverter<TElement> _arrayConverter;
    private readonly Func<TElement[], TCollection> _collectionFactory;

    /// <summary>
    /// Initializes a new instance with an optional per-element converter and a collection factory.
    /// </summary>
    /// <param name="elementConverter">Per-element converter; <see langword="null" /> means string pass-through.</param>
    /// <param name="collectionFactory">Factory that constructs the target collection from the element array.</param>
    public ArrayInitializableSequenceBindingConverter(
        BindingConverter<TElement>? elementConverter,
        Func<TElement[], TCollection> collectionFactory
    )
    {
        _arrayConverter = new ArraySequenceBindingConverter<TElement>(elementConverter);
        _collectionFactory = collectionFactory;
    }

    /// <inheritdoc />
    public override TCollection ConvertMany(IReadOnlyList<string?> rawValues)
    {
        var array = _arrayConverter.ConvertMany(rawValues);
        return _collectionFactory(array);
    }
}
