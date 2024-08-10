using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding inputs to properties without any conversion.
/// </summary>
public class NoopBindingConverter : IBindingConverter
{
    /// <inheritdoc />
    public object? Convert(string? rawValue, IFormatProvider? formatProvider) => rawValue;
}
