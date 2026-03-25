using System.Collections.Generic;

namespace CliFx.Activation;

/// <inheritdoc />
public abstract class InputConverter<T> : IInputConverter<T>
{
    /// <inheritdoc />
    public abstract bool CanConvertSequence { get; }

    /// <inheritdoc cref="IInputConverter.Convert" />
    public abstract T Convert(IReadOnlyList<string> rawValues);

    object? IInputConverter.Convert(IReadOnlyList<string> rawValues) => Convert(rawValues);
}
