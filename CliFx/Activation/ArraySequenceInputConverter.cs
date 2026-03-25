using System.Collections.Generic;
using System.Linq;

namespace CliFx.Activation;

/// <summary>
/// Sequence converter for activating command inputs bound to properties of array types.
/// </summary>
public class ArraySequenceInputConverter<TElement>(ScalarInputConverter<TElement> elementConverter)
    : SequenceInputConverter<TElement[]>
{
    /// <inheritdoc />
    public override TElement[] Convert(IReadOnlyList<string> rawValues)
    {
        var result = new TElement[rawValues.Count];
        foreach (var (i, rawValue) in rawValues.Index())
            result[i] = elementConverter.Convert(rawValue);

        return result;
    }
}
