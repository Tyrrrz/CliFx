namespace CliFx.Extensibility;

/// <summary>
/// Defines custom validation logic for activated command inputs.
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

    /// <inheritdoc cref="IBindingValidator.Validate" />
    /// <remarks>
    /// You can use the utility methods <see cref="Ok" /> and <see cref="Error" /> to
    /// create an appropriate result.
    /// </remarks>
    public abstract BindingValidationError? Validate(T? value);

    BindingValidationError? IBindingValidator.Validate(object? value) => Validate((T?)value);
}
