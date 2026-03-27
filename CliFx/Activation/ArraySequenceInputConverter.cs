using System.Collections.Generic;
using System.Linq;

namespace CliFx.Activation;

/// <summary>
/// Sequence converter for activating command inputs bound to properties of array types.
/// </summary>
public class ArraySequenceInputConverter<T>(ScalarInputConverter<T> elementConverter)
    : SequenceInputConverter<T[]>
{
    /// <inheritdoc />
    public override T[] Convert(IReadOnlyList<string> rawValues)
    {
        var result = new T[rawValues.Count];
        foreach (var (i, rawValue) in rawValues.Index())
            result[i] = elementConverter.Convert(rawValue);

        return result;
    }
}

/// <summary>
/// Utilities for creating <see cref="ArraySequenceInputConverter{T}" />.
/// </summary>
public static class ArraySequenceInputConverter
{
    /// <summary>
    /// Creates an array-based sequence converter.
    /// </summary>
    public static ArraySequenceInputConverter<T> Create<T>(
        ScalarInputConverter<T> elementConverter
    ) => new(elementConverter);
}
