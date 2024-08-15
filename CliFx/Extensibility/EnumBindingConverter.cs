using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding command inputs to properties of type <see cref="Enum" />.
/// </summary>
public class EnumBindingConverter<T> : BindingConverter<T>
    where T : struct, Enum
{
    /// <inheritdoc />
    public override T Convert(string? rawValue, IFormatProvider? formatProvider) =>
        (T)Enum.Parse(typeof(T), rawValue!, true);
}
