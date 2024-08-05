using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for binding inputs to properties using a custom delegate.
/// </summary>
public class DelegateBindingConverter<T>(Func<string?, T> convert) : BindingConverter<T>
{
    /// <inheritdoc />
    public override T? Convert(string? rawValue) => convert(rawValue);
}