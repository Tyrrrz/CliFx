using System;
using System.Globalization;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="TimeSpan" />.
/// </summary>
public class TimeSpanBindingConverter : BindingConverter<TimeSpan>
{
    /// <inheritdoc />
    public override TimeSpan Convert(string? rawValue) =>
        TimeSpan.Parse(rawValue!, CultureInfo.InvariantCulture);
}
