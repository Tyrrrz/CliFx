using System;

namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties using a custom delegate.
/// </summary>
public class DelegateScalarInputConverter<T>(Func<string?, T> convert) : ScalarInputConverter<T>
{
    /// <inheritdoc />
    public override T Convert(string? rawValue) => convert(rawValue);
}
