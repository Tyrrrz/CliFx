using System.Collections.Generic;
using System.Linq;

namespace CliFx.Activation;

/// <summary>
/// Sequence converter for activating command inputs bound to properties whose type can be
/// assigned from an array of elements.
/// </summary>
public class ArraySequenceInputConverter<TElement, TSequence>(
    ScalarInputConverter<TElement> elementConverter
) : SequenceInputConverter<TSequence>
    where TSequence : IEnumerable<TElement>
{
    /// <inheritdoc />
    public override TSequence Convert(IReadOnlyList<string> rawValues)
    {
        var result = new TElement[rawValues.Count];
        for (var i = 0; i < rawValues.Count; i++)
            result[i] = elementConverter.Convert(rawValues[i]);

        // Validity of this call is ensured by the instantiator of this converter
        return (TSequence)result.AsEnumerable();
    }
}

/// <summary>
/// Sequence converter for activating command inputs bound to properties whose type can be
/// assigned from an array of elements.
/// </summary>
public class ArraySequenceInputConverter<TElement>(ScalarInputConverter<TElement> elementConverter)
    : ArraySequenceInputConverter<TElement, TElement[]>(elementConverter);
