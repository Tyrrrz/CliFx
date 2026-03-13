using System;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="Enum" />.
/// </summary>
public class EnumBindingConverter<T> : BindingConverter<T>
    where T : struct, Enum
{
    /// <inheritdoc />
    public override T Convert(string? rawValue) => (T)Enum.Parse(typeof(T), rawValue!, true);
}
