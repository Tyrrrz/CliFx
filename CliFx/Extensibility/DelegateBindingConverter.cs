using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding inputs to properties using a custom delegate.
/// </summary>
public class DelegateBindingConverter<T>(Func<string?, IFormatProvider?, T> convert)
    : BindingConverter<T>
{
    /// <summary>
    /// Initializes an instance of <see cref="DelegateBindingConverter{T}" />
    /// </summary>
    public DelegateBindingConverter(Func<string?, T> convert)
        : this((rawValue, _) => convert(rawValue)) { }

    /// <inheritdoc />
    public override T Convert(string? rawValue, IFormatProvider? formatProvider) =>
        convert(rawValue, formatProvider);
}
