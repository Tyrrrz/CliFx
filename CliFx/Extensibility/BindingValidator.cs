using CliFx.Exceptions;

namespace CliFx.Extensibility
{
    // Used internally to simplify usage from reflection
    internal interface IBindingValidator
    {
        void Validate(object? value);
    }

    /// <summary>
    /// Base type for custom validators.
    /// </summary>
    public abstract class BindingValidator<T> : IBindingValidator
    {
        /// <summary>
        /// Validates the value bound to a parameter or an option.
        /// </summary>
        /// <remarks>
        /// Throw <see cref="CommandException"/> to signal validation failure.
        /// </remarks>
        public abstract void Validate(T value);

        void IBindingValidator.Validate(object? value) => Validate((T) value!);
    }
}
