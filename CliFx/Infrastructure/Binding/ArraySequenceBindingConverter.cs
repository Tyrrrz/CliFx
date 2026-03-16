using System.Collections.Generic;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Sequence converter for activating command inputs bound to properties whose type can be
/// assigned from an array of elements.
/// </summary>
public class ArraySequenceBindingConverter<TElement, TSequence>(
    ScalarBindingConverter<TElement> elementConverter
) : SequenceBindingConverter<TSequence>
    where TSequence : IEnumerable<TElement>
{
    /// <inheritdoc />
    public override TSequence Convert(IReadOnlyList<string> rawValues)
    {
        var result = new TElement[rawValues.Count];
        for (var i = 0; i < rawValues.Count; i++)
            result[i] = elementConverter.Convert(rawValues[i]);

        // There are no generic constraints that would ensure that TSequence
        // is assignable from TElement[], so we assume the caller knows what
        // they're doing.
        return (TSequence)(object)result;
    }
}

/// <summary>
/// Sequence converter for activating command inputs bound to properties whose type can be
/// assigned from an array of elements.
/// </summary>
public class ArraySequenceBindingConverter<TElement>(
    ScalarBindingConverter<TElement> elementConverter
) : ArraySequenceBindingConverter<TElement, TElement[]>(elementConverter);
