using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding command inputs to properties of type <see cref="bool" />.
/// </summary>
public class BoolBindingConverter : BindingConverter<bool>
{
    /// <inheritdoc />
    public override bool Convert(string? rawArgument, IFormatProvider? formatProvider) =>
        string.IsNullOrWhiteSpace(rawArgument) || bool.Parse(rawArgument);
}
