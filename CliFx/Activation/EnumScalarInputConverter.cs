using System;

namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="Enum" />.
/// </summary>
public class EnumScalarInputConverter<T> : ScalarInputConverter<T>
    where T : struct, Enum
{
    /// <inheritdoc />
    public override T Convert(string? rawValue) => (T)Enum.Parse(typeof(T), rawValue!, true);
}
