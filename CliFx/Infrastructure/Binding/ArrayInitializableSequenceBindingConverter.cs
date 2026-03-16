using System;
using System.Collections.Generic;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Sequence converter for activating command inputs bound to properties whose type can be
/// initialized from an array of elements.
/// </summary>
public class ArrayInitializableSequenceBindingConverter<TElement, TSequence>(
    ScalarBindingConverter<TElement> elementConverter,
    Func<TElement[], TSequence> fromArray
) : SequenceBindingConverter<TSequence>
    where TSequence : IEnumerable<TElement>
{
    /// <inheritdoc />
    public override TSequence Convert(IReadOnlyList<string> rawValues) =>
        fromArray(new ArraySequenceBindingConverter<TElement>(elementConverter).Convert(rawValues));
}
