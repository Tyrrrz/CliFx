using System;

namespace CliFx.Extensibility;

/// <summary>
/// Base type for custom converters.
/// </summary>
public abstract class BindingConverter<T> : IBindingConverter
{
    /// <summary>
    /// Parses the value from a raw command-line argument.
    /// </summary>
    public abstract T? Convert(string? rawValue, IFormatProvider? formatProvider);

    object? IBindingConverter.Convert(string? rawValue, IFormatProvider? formatProvider) =>
        Convert(rawValue, formatProvider);
}
