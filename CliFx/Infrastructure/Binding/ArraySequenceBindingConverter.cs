using System;
using System.Collections.Generic;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Collection converter that produces a <typeparamref name="TElement" /> array by applying an optional
/// per-element <see cref="BindingConverter{T}" /> and then casts it to
/// <typeparamref name="TSequence" />.  This works for any <typeparamref name="TSequence" /> that
/// is assignable from <typeparamref name="TElement" />[], such as
/// <see cref="System.Collections.Generic.IEnumerable{T}" />,
/// <see cref="System.Collections.Generic.IReadOnlyList{T}" />, or <typeparamref name="TElement" />[].
/// </summary>
public class ArraySequenceBindingConverter<TElement, TSequence>
    : SequenceBindingConverter<TSequence>
{
    private readonly BindingConverter<TElement>? _elementConverter;

    /// <summary>
    /// Initializes a new instance with an optional per-element converter.
    /// When <paramref name="elementConverter" /> is <see langword="null" />, raw values are passed
    /// through as-is; <typeparamref name="TElement" /> must be <see cref="string" /> in that case.
    /// </summary>
    public ArraySequenceBindingConverter(BindingConverter<TElement>? elementConverter = null)
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
    public override TSequence ConvertMany(IReadOnlyList<string?> rawValues)
    {
        var result = new TElement[rawValues.Count];
        for (var i = 0; i < rawValues.Count; i++)
        {
            // When _elementConverter is null, TElement is always string (enforced by the constructor).
            result[i] = _elementConverter is not null
                ? _elementConverter.Convert(rawValues[i])
                : (TElement)(object)(rawValues[i]!);
        }

        return (TSequence)(object)result;
    }
}

/// <summary>
/// Convenience specialization of <see cref="ArraySequenceBindingConverter{TElement, TSequence}" />
/// where <typeparamref name="TElement" />[] is both the element container and the target collection type.
/// </summary>
public class ArraySequenceBindingConverter<TElement>
    : ArraySequenceBindingConverter<TElement, TElement[]>
{
    /// <inheritdoc />
    public ArraySequenceBindingConverter(BindingConverter<TElement>? elementConverter = null)
        : base(elementConverter) { }
}
