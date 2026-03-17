using System;
using System.Globalization;

namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties whose types implement <see cref="IConvertible" />.
/// </summary>
public class ConvertibleScalarInputConverter<T> : ScalarInputConverter<T>
    where T : IConvertible
{
    /// <inheritdoc />
    public override T Convert(string? rawValue) =>
        (T)System.Convert.ChangeType(rawValue, typeof(T), CultureInfo.InvariantCulture)!;
}
