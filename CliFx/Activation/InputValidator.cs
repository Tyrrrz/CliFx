using System.Collections.Generic;

namespace CliFx.Activation;

/// <inheritdoc />
public abstract class InputValidator<T> : IInputValidator<T>
{
    /// <summary>
    /// Returns a validation error.
    /// </summary>
    protected InputValidationError Error(string message) => new(message);

    /// <inheritdoc cref="IInputValidator.Validate" />
    public abstract IEnumerable<InputValidationError> Validate(T value);

    IEnumerable<InputValidationError> IInputValidator.Validate(object? value)
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
