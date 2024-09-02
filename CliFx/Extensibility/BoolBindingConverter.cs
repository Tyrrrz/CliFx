using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="bool" />.
/// </summary>
public class BoolBindingConverter : BindingConverter<bool>
{
    /// <inheritdoc />
    public override bool Convert(string? rawValue, IFormatProvider? formatProvider) =>
        string.IsNullOrWhiteSpace(rawValue) || bool.Parse(rawValue);
}
