using System;

namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs using a custom delegate.
/// </summary>
public class DelegateScalarInputConverter<T>(Func<string?, T> convert) : ScalarInputConverter<T>
{
    /// <inheritdoc />
    public override T Convert(string? rawValue) => convert(rawValue);
}

/// <summary>
/// Converter for activating command inputs using another scalar converter and a transform delegate.
/// </summary>
public class DelegateScalarInputConverter<TInner, T>(
    ScalarInputConverter<TInner> innerConverter,
    Func<TInner, T> transform
) : ScalarInputConverter<T>
{
    /// <inheritdoc />
    public override T Convert(string? rawValue) => transform(innerConverter.Convert(rawValue));
}
