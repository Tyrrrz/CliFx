namespace CliFx.Activation;

/// <inheritdoc />
public abstract class InputValidator<T> : IInputValidator<T>
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
    public abstract InputValidationError? Validate(T value);

    InputValidationError? IInputValidator.Validate(object? value)
    {
        // Value is of the expected type
        if (value is T valueAsT)
            return Validate(valueAsT);

        // Value is null and the expected type is nullable
        if (value is null && default(T) is null)
            return Validate(default!);

        // Type mismatch
        throw CliFxException.InternalError(
            $"Expected a value of type '{typeof(T)}', but provided with '{value?.GetType()}'."
        );
    }
}
