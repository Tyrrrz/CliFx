using System;
using System.Collections.Generic;

namespace CliFx.Activation;

/// <summary>
/// Sequence converter for activating command inputs using a custom delegate.
/// </summary>
public class DelegateSequenceInputConverter<T>(Func<IReadOnlyList<string>, T> convert)
    : SequenceInputConverter<T>
{
    /// <inheritdoc />
    public override T Convert(IReadOnlyList<string> rawValues) => convert(rawValues);
}

/// <summary>
/// Utilities for creating <see cref="DelegateSequenceInputConverter{T}" />.
/// </summary>
public static class DelegateSequenceInputConverter
{
    /// <summary>
    /// Creates a delegate-based sequence converter.
    /// </summary>
    public static DelegateSequenceInputConverter<T> Create<T>(
        Func<IReadOnlyList<string>, T> convert
    ) => new(convert);

    /// <summary>
    /// Creates a delegate-based sequence converter.
    /// </summary>
    public static DelegateSequenceInputConverter<T> Create<T, TInner>(
        SequenceInputConverter<TInner> innerConverter,
        Func<TInner, T> transform
    ) => Create(vs => transform(innerConverter.Convert(vs)));
}
