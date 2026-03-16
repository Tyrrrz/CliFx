using System.Collections.Generic;

namespace CliFx.Infrastructure.Binding;

/// <inheritdoc />
public abstract class BindingConverter<T> : IBindingConverter
{
    /// <inheritdoc />
    public abstract bool IsSequence { get; }

    /// <inheritdoc cref="IBindingConverter.Convert" />
    public abstract T Convert(IReadOnlyList<string> rawValues);

    object? IBindingConverter.Convert(IReadOnlyList<string> rawValues) => Convert(rawValues);
}
