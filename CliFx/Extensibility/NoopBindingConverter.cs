using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding command inputs to properties without any conversion.
/// </summary>
public class NoopBindingConverter : IBindingConverter
{
    /// <inheritdoc />
    public object? Convert(string? rawArgument, IFormatProvider? formatProvider) => rawArgument;
}
