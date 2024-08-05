using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding inputs to properties of type <see cref="DateTimeOffset" />.
/// </summary>
public class DateTimeOffsetBindingConverter(IFormatProvider formatProvider) : BindingConverter<DateTimeOffset>
{
    /// <inheritdoc />
    public override DateTimeOffset Convert(string? rawValue) => DateTimeOffset.Parse(rawValue!, formatProvider);
}