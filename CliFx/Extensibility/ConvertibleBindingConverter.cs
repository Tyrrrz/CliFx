using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding command inputs to properties whose types implement <see cref="IConvertible" />.
/// </summary>
public class ConvertibleBindingConverter<T> : BindingConverter<T>
    where T : IConvertible
{
    /// <inheritdoc />
    public override T? Convert(string? rawArgument, IFormatProvider? formatProvider) =>
        (T?)System.Convert.ChangeType(rawArgument, typeof(T), formatProvider);
}
