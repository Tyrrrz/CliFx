namespace CliFx.Activation;

/// <inheritdoc />
public abstract class InputValidator<T> : IInputValidator
{
    /// <summary>
    /// Returns a successful validation result.
    /// </summary>
    protected InputValidationError? Ok() => null;

    /// <summary>
    /// Returns a non-successful validation result.
    /// </summary>
    protected InputValidationError Error(string message) => new(message);

    /// <inheritdoc cref="IInputValidator.Validate" />
    public abstract InputValidationError? Validate(T? value);

    InputValidationError? IInputValidator.Validate(object? value) => Validate((T?)value);
}
