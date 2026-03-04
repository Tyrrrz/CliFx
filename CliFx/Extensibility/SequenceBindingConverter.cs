using System.Collections.Generic;

namespace CliFx.Extensibility;

/// <summary>
/// Defines custom conversion logic for activating sequential command inputs
/// from the corresponding multiple raw command-line arguments.
/// </summary>
/// <remarks>
/// To implement your own sequence converter, inherit from <see cref="SequenceBindingConverter{T}" /> instead.
/// </remarks>
public interface ISequenceBindingConverter
{
    /// <summary>
    /// Parses values from multiple raw command-line arguments.
    /// </summary>
    object? ConvertMany(IReadOnlyList<string?> rawValues);
}

/// <summary>
/// Base type for custom sequence converters.
/// </summary>
public abstract class SequenceBindingConverter<T> : ISequenceBindingConverter
{
    /// <inheritdoc cref="ISequenceBindingConverter.ConvertMany" />
    public abstract T ConvertMany(IReadOnlyList<string?> rawValues);

    object? ISequenceBindingConverter.ConvertMany(IReadOnlyList<string?> rawValues) =>
        ConvertMany(rawValues);
}
