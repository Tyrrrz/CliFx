using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding inputs to properties of type <see cref="bool" />.
/// </summary>
public class BoolBindingConverter : BindingConverter<bool>
{
    /// <inheritdoc />
    public override bool Convert(string? rawValue, IFormatProvider? formatProvider) =>
        string.IsNullOrWhiteSpace(rawValue) || bool.Parse(rawValue);
}
