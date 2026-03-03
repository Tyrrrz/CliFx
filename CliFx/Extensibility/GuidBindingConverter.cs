using System;

namespace CliFx.Extensibility;

/// <summary>Converter for activating command inputs bound to properties of type <see cref="Guid"/>.</summary>
public class GuidBindingConverter : BindingConverter<Guid>
{
    /// <inheritdoc />
    public override Guid Convert(string? rawValue) => Guid.Parse(rawValue!);
}
