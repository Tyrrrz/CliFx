namespace CliFx.Activation;

/// <summary>
/// Defines validation logic for activated command inputs.
/// </summary>
/// <remarks>
/// To implement your own validator, inherit from <see cref="InputValidator{T}" />.
/// </remarks>
public interface IInputValidator
{
    /// <summary>
    /// Validates the input value.
    /// Returns <c>null</c> if the validation is successful, or an error in case of failure.
    /// </summary>
    InputValidationError? Validate(object? value);
}

/// <inheritdoc />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
// This interface is a bit messy but is required for covariance, which helps keep the source generators simpler
public interface IInputValidator<T> : IInputValidator
{
    /// <inheritdoc cref="IInputValidator.Validate" />
    InputValidationError? Validate(T value);
}
