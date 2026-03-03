using System;
using System.Collections.Generic;

namespace CliFx.Extensibility;

/// <summary>
/// Defines custom conversion logic for activating sequence command inputs from multiple raw command-line arguments.
/// </summary>
/// <remarks>
/// To implement your own collection converter, inherit from <see cref="CollectionBindingConverter{T}" /> instead.
/// </remarks>
public interface ICollectionBindingConverter
{
    /// <summary>
    /// Converts multiple raw command-line argument values into the target collection type.
    /// </summary>
    object? ConvertMany(IReadOnlyList<string?> rawValues);
}

/// <summary>
/// Base type for custom collection converters.
/// </summary>
public abstract class CollectionBindingConverter<T> : ICollectionBindingConverter
{
    /// <summary>
    /// Converts multiple raw command-line argument values into the target collection type.
    /// </summary>
    public abstract T ConvertMany(IReadOnlyList<string?> rawValues);

    object? ICollectionBindingConverter.ConvertMany(IReadOnlyList<string?> rawValues) =>
        ConvertMany(rawValues);
}

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

/// <summary>
/// Collection converter that first converts raw values into a <typeparamref name="TElement" /> array
/// (via an <see cref="ArrayCollectionBindingConverter{TElement}" />) and then produces the final
/// <typeparamref name="TCollection" /> using a caller-supplied factory delegate.
/// Suitable for array-initializable concrete collection types such as <see cref="System.Collections.Generic.List{T}" />.
/// </summary>
public class ArrayInitializableCollectionBindingConverter<TElement, TCollection>(
    ArrayCollectionBindingConverter<TElement> arrayConverter,
    Func<TElement[], TCollection> collectionFactory
) : CollectionBindingConverter<TCollection>
{
    /// <inheritdoc />
    public override TCollection ConvertMany(IReadOnlyList<string?> rawValues)
    {
        var array = arrayConverter.ConvertMany(rawValues);
        return collectionFactory(array);
    }
}

/// <summary>
/// Collection converter that uses a custom delegate.
/// </summary>
public sealed class DelegateCollectionBindingConverter<T>(Func<IReadOnlyList<string?>, T> convert)
    : CollectionBindingConverter<T>
{
    /// <inheritdoc />
    public override T ConvertMany(IReadOnlyList<string?> rawValues) => convert(rawValues);
}
