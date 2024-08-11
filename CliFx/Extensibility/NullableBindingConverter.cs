using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding command inputs to properties of type <see cref="Nullable{T}" />.
/// </summary>
public class NullableBindingConverter<T>(BindingConverter<T> innerConverter) : BindingConverter<T?>
    where T : struct
{
    /// <inheritdoc />
    public override T? Convert(string? rawArgument, IFormatProvider? formatProvider) =>
        !string.IsNullOrWhiteSpace(rawArgument)
            ? innerConverter.Convert(rawArgument, formatProvider)
            : null;
}
