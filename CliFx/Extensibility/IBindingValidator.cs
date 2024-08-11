namespace CliFx.Extensibility;

/// <summary>
/// Defines custom validation logic for activated command inputs.
/// </summary>
/// <remarks>
/// To implement your own validator, inherit from <see cref="BindingValidator{T}" /> instead.
/// </remarks>
public interface IBindingValidator
{
    /// <summary>
    /// Validates the input value.
    /// Returns <c>null</c> if the validation is successful, or an error in case of failure.
    /// </summary>
    BindingValidationError? Validate(object? value);
}
