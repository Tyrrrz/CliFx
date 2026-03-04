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

/// <summary>
/// Base type for custom validators.
/// </summary>
public abstract class BindingValidator<T> : IBindingValidator
{
    /// <summary>
    /// Returns a successful validation result.
    /// </summary>
    protected BindingValidationError? Ok() => null;

    /// <summary>
    /// Returns a non-successful validation result.
    /// </summary>
    protected BindingValidationError Error(string message) => new(message);

    /// <summary>
    /// Validates the value bound to a parameter or an option.
    /// Returns null if validation is successful, or an error in case of failure.
    /// </summary>
    public abstract BindingValidationError? Validate(T? value);

    BindingValidationError? IBindingValidator.Validate(object? value) => Validate((T?)value);
}
