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
    object? ConvertMany(IReadOnlyList<string?> rawValues);
}

/// <summary>
/// Base type for custom collection converters.
/// </summary>
public abstract class CollectionBindingConverter<T> : ICollectionBindingConverter
{
    /// <summary>
    /// Converts multiple raw command-line argument values into the target collection type.
    /// </summary>
    public abstract T ConvertMany(IReadOnlyList<string?> rawValues);

    object? ICollectionBindingConverter.ConvertMany(IReadOnlyList<string?> rawValues) =>
        ConvertMany(rawValues);
}
