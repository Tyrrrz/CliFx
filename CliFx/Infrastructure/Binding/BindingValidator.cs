namespace CliFx.Infrastructure.Binding;

/// <inheritdoc />
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
    public abstract BindingValidationError? Validate(T? value);

    BindingValidationError? IBindingValidator.Validate(object? value) => Validate((T?)value);
}
