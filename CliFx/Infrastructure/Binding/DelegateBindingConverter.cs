using System;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Converter for activating command inputs bound to properties using a custom delegate.
/// </summary>
public class DelegateBindingConverter<T>(Func<string?, T> convert) : BindingConverter<T>
{
    /// <inheritdoc />
    public override T Convert(string? rawValue) => convert(rawValue);
}
