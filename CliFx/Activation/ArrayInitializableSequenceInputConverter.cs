using System;
using System.Collections.Generic;

namespace CliFx.Activation;

/// <summary>
/// Sequence converter for activating command inputs bound to properties whose type can be
/// initialized from an array of elements.
/// </summary>
public class ArrayInitializableSequenceInputConverter<TElement, TSequence>(
    ScalarInputConverter<TElement> elementConverter,
    Func<TElement[], TSequence> fromArray
) : SequenceInputConverter<TSequence>
    where TSequence : IEnumerable<TElement>
{
    /// <inheritdoc />
    public override TSequence Convert(IReadOnlyList<string> rawValues) =>
        fromArray(new ArraySequenceInputConverter<TElement>(elementConverter).Convert(rawValues));
}
