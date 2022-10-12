namespace CliFx.Extensibility;

// Used internally to simplify usage from reflection
internal interface IBindingValidator
{
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
    /// <remarks>
    /// You can use the utility methods <see cref="Ok" /> and <see cref="Error" /> to
    /// create an appropriate result.
    /// </remarks>
    public abstract BindingValidationError? Validate(T? value);

    BindingValidationError? IBindingValidator.Validate(object? value) => Validate((T?)value);
}