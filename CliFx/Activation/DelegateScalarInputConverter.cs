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
/// Utilities for creating <see cref="DelegateScalarInputConverter{T}" />.
/// </summary>
public static class DelegateScalarInputConverter
{
    /// <summary>
    /// Creates a delegate-based scalar converter.
    /// </summary>
    public static DelegateScalarInputConverter<T> Create<T>(Func<string?, T> convert) =>
        new(convert);

    /// <summary>
    /// Creates a delegate-based scalar converter.
    /// </summary>
    public static DelegateScalarInputConverter<T> Create<T, TInner>(
        ScalarInputConverter<TInner> innerConverter,
        Func<TInner, T> transform
    ) => Create(v => transform(innerConverter.Convert(v)));
}
