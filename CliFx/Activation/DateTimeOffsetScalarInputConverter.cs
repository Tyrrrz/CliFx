using System;
using System.Globalization;

namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="DateTimeOffset" />.
/// </summary>
public class DateTimeOffsetScalarInputConverter : ScalarInputConverter<DateTimeOffset>
{
    /// <inheritdoc />
    public override DateTimeOffset Convert(string? rawValue) =>
        DateTimeOffset.Parse(rawValue!, CultureInfo.InvariantCulture);
}
