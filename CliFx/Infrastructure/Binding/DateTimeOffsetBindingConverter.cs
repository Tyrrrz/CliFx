using System;
using System.Globalization;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="DateTimeOffset" />.
/// </summary>
public class DateTimeOffsetBindingConverter : BindingConverter<DateTimeOffset>
{
    /// <inheritdoc />
    public override DateTimeOffset Convert(string? rawValue) =>
        DateTimeOffset.Parse(rawValue!, CultureInfo.InvariantCulture);
}
