using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for activating command inputs bound to properties without performing any conversion.
/// </summary>
public class NoopBindingConverter : IBindingConverter
{
    /// <inheritdoc />
    public object? Convert(string? rawValue, IFormatProvider? formatProvider) => rawValue;
}
