namespace CliFx.Extensibility;

/// <summary>
/// Defines a custom conversion for binding command-line arguments to command inputs.
/// </summary>
/// <remarks>
/// To implement your own converter, inherit from <see cref="BindingConverter{T}" /> instead.
/// </remarks>
public interface IBindingConverter
{
    /// <summary>
    /// Parses the value from a raw command-line argument.
    /// </summary>
    object? Convert(string? rawValue);
}