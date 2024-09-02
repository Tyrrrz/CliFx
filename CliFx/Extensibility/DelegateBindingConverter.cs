using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for activating command inputs bound to properties using a custom delegate.
/// </summary>
public class DelegateBindingConverter<T>(Func<string?, IFormatProvider?, T> convert)
    : BindingConverter<T>
{
    /// <summary>
    /// Initializes an instance of <see cref="DelegateBindingConverter{T}" />
    /// </summary>
    public DelegateBindingConverter(Func<string?, T> convert)
        : this((rawArgument, _) => convert(rawArgument)) { }

    /// <inheritdoc />
    public override T Convert(string? rawValue, IFormatProvider? formatProvider) =>
        convert(rawValue, formatProvider);
}
