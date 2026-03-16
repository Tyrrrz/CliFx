using System.Collections.Generic;

namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Defines conversion logic used for activating command inputs from raw command-line arguments.
/// </summary>
/// <remarks>
/// To implement your own converter, inherit from <see cref="ScalarBindingConverter{T}" /> for
/// scalar (single-value) types and from <see cref="SequenceBindingConverter{T}" /> for
/// sequence-based (multi-value) types.
/// </remarks>
public interface IBindingConverter
{
    /// <summary>
    /// Whether this converter is meant to be used with sequence-based (multi-value) command inputs.
    /// </summary>
    /// <remarks>
    /// If this property is <c>false</c>, then <see cref="Convert" /> must throw when provided
    /// with more than one value.
    /// </remarks>
    bool IsSequence { get; }

    /// <summary>
    /// Parses the input value from the provided command-line arguments.
    /// </summary>
    object? Convert(IReadOnlyList<string> rawValues);
}
