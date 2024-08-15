using System;

namespace CliFx.Extensibility;

/// <summary>
/// Defines custom conversion logic for activating command inputs from the corresponding raw command-line arguments.
/// </summary>
public abstract class BindingConverter<T> : IBindingConverter
{
    /// <inheritdoc cref="IBindingConverter.Convert" />
    public abstract T? Convert(string? rawValue, IFormatProvider? formatProvider);

    object? IBindingConverter.Convert(string? rawValue, IFormatProvider? formatProvider) =>
        Convert(rawValue, formatProvider);
}
