using System;
using System.Collections.Generic;

namespace CliFx.Extensibility;

/// <summary>
/// Collection converter that produces a <typeparamref name="TElement" /> array by applying an optional
/// per-element <see cref="BindingConverter{T}" />.  The resulting array is assignable to
/// <see cref="System.Collections.Generic.IEnumerable{T}" />, <see cref="System.Collections.Generic.IReadOnlyList{T}" />,
/// and any other interface implemented by arrays.
/// </summary>
public class ArrayCollectionBindingConverter<TElement> : CollectionBindingConverter<TElement[]>
{
    private readonly BindingConverter<TElement>? _elementConverter;

    /// <summary>
    /// Initializes a new instance with an optional per-element converter.
    /// When <paramref name="elementConverter" /> is <see langword="null" />, raw values are passed
    /// through as-is; <typeparamref name="TElement" /> must be <see cref="string" /> in that case.
    /// </summary>
    public ArrayCollectionBindingConverter(BindingConverter<TElement>? elementConverter = null)
    {
        if (elementConverter is null && typeof(TElement) != typeof(string))
        {
            throw new InvalidOperationException(
                $"A null element converter is only valid when '{nameof(TElement)}' is '{typeof(string).FullName}', "
                    + $"but '{typeof(TElement).FullName}' was provided. Supply a '{nameof(BindingConverter<TElement>)}' instance."
            );
        }

        _elementConverter = elementConverter;
    }

    /// <inheritdoc />
    public override TElement[] ConvertMany(IReadOnlyList<string?> rawValues)
    {
        var result = new TElement[rawValues.Count];
        for (var i = 0; i < rawValues.Count; i++)
        {
            // When _elementConverter is null, TElement is always string (enforced by the constructor).
            result[i] = _elementConverter is not null
                ? _elementConverter.Convert(rawValues[i])
                : (TElement)(object)(rawValues[i]!);
        }

        return result;
    }
}
