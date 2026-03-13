using System;
using System.Collections.Generic;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Collection converter that first converts raw values into a <typeparamref name="TElement" /> array
/// (via an <see cref="ArraySequenceBindingConverter{TElement}" /> created internally) and then produces
/// the final <typeparamref name="TSequence" /> using a caller-supplied factory delegate.
/// Suitable for array-initializable concrete collection types such as <see cref="System.Collections.Generic.List{T}" />.
/// </summary>
public class ArrayInitializableSequenceBindingConverter<TElement, TSequence>(
    BindingConverter<TElement>? elementConverter,
    Func<TElement[], TSequence> fromArray
) : SequenceBindingConverter<TSequence>
{
    private readonly ArraySequenceBindingConverter<TElement> _arrayConverter = new(
        elementConverter
    );

    /// <inheritdoc />
    public override TSequence ConvertMany(IReadOnlyList<string?> rawValues) =>
        fromArray(_arrayConverter.ConvertMany(rawValues));
}
