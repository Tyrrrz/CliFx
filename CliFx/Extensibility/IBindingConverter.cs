using System;

namespace CliFx.Extensibility;

/// <summary>
/// Defines custom conversion logic for activating command inputs from the corresponding raw command-line arguments.
/// </summary>
/// <remarks>
/// To implement your own converter, inherit from <see cref="BindingConverter{T}" /> instead.
/// </remarks>
public interface IBindingConverter
{
    /// <summary>
    /// Parses the value from a raw command-line argument.
    /// </summary>
    object? Convert(string? rawArgument, IFormatProvider? formatProvider);
}
