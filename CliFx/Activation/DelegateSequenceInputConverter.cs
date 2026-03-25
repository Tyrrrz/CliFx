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
/// Sequence converter for activating command inputs using another sequence converter and a transform delegate.
/// </summary>
public class DelegateSequenceInputConverter<TInner, T>(
    SequenceInputConverter<TInner> innerConverter,
    Func<TInner, T> transform
) : SequenceInputConverter<T>
{
    /// <inheritdoc />
    public override T Convert(IReadOnlyList<string> rawValues) =>
        transform(innerConverter.Convert(rawValues));
}
