using System;
using System.Globalization;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Converter for activating command inputs bound to properties whose types implement <see cref="IConvertible" />.
/// </summary>
public class ConvertibleScalarBindingConverter<T> : ScalarBindingConverter<T>
    where T : IConvertible
{
    /// <inheritdoc />
    public override T Convert(string? rawValue) =>
        (T)System.Convert.ChangeType(rawValue, typeof(T), CultureInfo.InvariantCulture)!;
}
