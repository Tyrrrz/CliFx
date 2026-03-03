using System;
using System.Collections.Generic;

namespace CliFx.Extensibility;

/// <summary>
/// Defines custom conversion logic for activating sequence command inputs from multiple raw command-line arguments.
/// </summary>
/// <remarks>
/// To implement your own collection converter, inherit from <see cref="CollectionBindingConverter{T}" /> instead.
/// </remarks>
public interface ICollectionBindingConverter
{
    /// <summary>
    /// Converts multiple raw command-line argument values into the target collection type.
    /// </summary>
    object? ConvertCollection(IReadOnlyList<string?> rawValues);
}

/// <summary>
/// Base type for custom collection converters.
/// </summary>
public abstract class CollectionBindingConverter<T> : ICollectionBindingConverter
{
    /// <summary>
    /// Converts multiple raw command-line argument values into the target collection type.
    /// </summary>
    public abstract T ConvertCollection(IReadOnlyList<string?> rawValues);

    object? ICollectionBindingConverter.ConvertCollection(IReadOnlyList<string?> rawValues) =>
        ConvertCollection(rawValues);
}

/// <summary>
/// Collection converter that uses a custom delegate.
/// </summary>
public sealed class DelegateCollectionBindingConverter<T>(Func<IReadOnlyList<string?>, T> convert)
    : CollectionBindingConverter<T>
{
    /// <inheritdoc />
    public override T ConvertCollection(IReadOnlyList<string?> rawValues) => convert(rawValues);
}
