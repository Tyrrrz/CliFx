using System;
using System.Globalization;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="DateTime" />.
/// </summary>
public class DateTimeScalarBindingConverter : ScalarBindingConverter<DateTime>
{
    /// <inheritdoc />
    public override DateTime Convert(string? rawValue) =>
        DateTime.Parse(rawValue!, CultureInfo.InvariantCulture);
}
