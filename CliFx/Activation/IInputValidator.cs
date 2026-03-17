namespace CliFx.Activation;

/// <summary>
/// Defines validation logic for activated command inputs.
/// </summary>
/// <remarks>
/// To implement your own validator, inherit from <see cref="InputValidator{T}" /> instead.
/// </remarks>
public interface IInputValidator
{
    /// <summary>
    /// Validates the input value.
    /// Returns <c>null</c> if the validation is successful, or an error in case of failure.
    /// </summary>
    InputValidationError? Validate(object? value);
}
