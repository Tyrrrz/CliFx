using System.Collections.Generic;

namespace CliFx.Activation;

/// <summary>
/// Defines conversion logic used for activating command inputs from raw command-line arguments.
/// </summary>
/// <remarks>
/// To implement your own converter, inherit from <see cref="ScalarInputConverter{T}" /> for
/// scalar (single-value) types and from <see cref="SequenceInputConverter{T}" /> for
/// sequence-based (multi-value) types.
/// </remarks>
public interface IInputConverter
{
    /// <summary>
    /// Whether this converter can be used with sequence-based (multi-value) command inputs.
    /// </summary>
    /// <remarks>
    /// If this property is <c>false</c>, then <see cref="Convert" /> must throw when provided
    /// with more than one value.
    /// </remarks>
    bool SupportsSequence { get; }

    /// <summary>
    /// Converts the input value from the provided raw command-line arguments.
    /// </summary>
    object? Convert(IReadOnlyList<string> rawValues);
}
