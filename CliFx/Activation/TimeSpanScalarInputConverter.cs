using System;
using System.Globalization;

namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="TimeSpan" />.
/// </summary>
public class TimeSpanScalarInputConverter : ScalarInputConverter<TimeSpan>
{
    /// <inheritdoc />
    public override TimeSpan Convert(string? rawValue) =>
        TimeSpan.Parse(rawValue!, CultureInfo.InvariantCulture);
}
