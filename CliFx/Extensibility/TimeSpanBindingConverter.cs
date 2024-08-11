using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding command inputs to properties of type <see cref="TimeSpan" />.
/// </summary>
public class TimeSpanBindingConverter : BindingConverter<TimeSpan>
{
    /// <inheritdoc />
    public override TimeSpan Convert(string? rawArgument, IFormatProvider? formatProvider) =>
        TimeSpan.Parse(rawArgument!, formatProvider);
}
