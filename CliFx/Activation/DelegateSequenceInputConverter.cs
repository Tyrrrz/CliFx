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
