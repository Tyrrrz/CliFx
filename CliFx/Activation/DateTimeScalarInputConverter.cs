using System;
using System.Globalization;

namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="DateTime" />.
/// </summary>
public class DateTimeScalarInputConverter : ScalarInputConverter<DateTime>
{
    /// <inheritdoc />
    public override DateTime Convert(string? rawValue) =>
        DateTime.Parse(rawValue!, CultureInfo.InvariantCulture);
}
