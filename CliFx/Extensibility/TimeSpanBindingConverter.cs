using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding inputs to properties of type <see cref="TimeSpan" />.
/// </summary>
public class TimeSpanBindingConverter(IFormatProvider formatProvider) : BindingConverter<TimeSpan>
{
    /// <inheritdoc />
    public override TimeSpan Convert(string? rawValue) =>
        TimeSpan.Parse(rawValue!, formatProvider);
}