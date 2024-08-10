namespace CliFx.Extensibility;

/// <summary>
/// Defines a custom validation rules for values bound from command-line arguments.
/// </summary>
/// <remarks>
/// To implement your own validator, inherit from <see cref="BindingValidator{T}" /> instead.
/// </remarks>
public interface IBindingValidator
{
    /// <summary>
    /// Validates the value bound to a parameter or an option.
    /// Returns null if validation is successful, or an error in case of failure.
    /// </summary>
    BindingValidationError? Validate(object? value);
}
