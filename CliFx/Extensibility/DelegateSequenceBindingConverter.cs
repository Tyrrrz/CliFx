using System;
using System.Collections.Generic;

namespace CliFx.Extensibility;

/// <summary>
/// Collection converter that uses a custom delegate.
/// </summary>
public sealed class DelegateSequenceBindingConverter<T>(Func<IReadOnlyList<string?>, T> convert)
    : SequenceBindingConverter<T>
{
    /// <inheritdoc />
    public override T ConvertMany(IReadOnlyList<string?> rawValues) => convert(rawValues);
}
