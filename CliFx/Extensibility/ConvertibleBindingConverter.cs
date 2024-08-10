using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding inputs to properties that implement <see cref="IConvertible" />.
/// </summary>
public class ConvertibleBindingConverter<T> : BindingConverter<T>
    where T : IConvertible
{
    /// <inheritdoc />
    public override T? Convert(string? rawValue, IFormatProvider? formatProvider) =>
        (T?)System.Convert.ChangeType(rawValue, typeof(T), formatProvider);
}
