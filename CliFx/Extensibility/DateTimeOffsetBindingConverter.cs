using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding command inputs to properties of type <see cref="DateTimeOffset" />.
/// </summary>
public class DateTimeOffsetBindingConverter : BindingConverter<DateTimeOffset>
{
    /// <inheritdoc />
    public override DateTimeOffset Convert(string? rawArgument, IFormatProvider? formatProvider) =>
        DateTimeOffset.Parse(rawArgument!, formatProvider);
}
