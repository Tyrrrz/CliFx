using CliFx.Exceptions;

namespace CliFx.Extensibility
{
    // Used internally to simplify usage from reflection
    internal interface IArgumentValidator
    {
        void Validate(object? value);
    }

    /// <summary>
    /// Base type for custom validators.
    /// </summary>
    public abstract class ArgumentValidator<T> : IArgumentValidator
    {
        /// <summary>
        /// Validates the value bound to a parameter or an option.
        /// </summary>
        /// <remarks>
        /// Throw <see cref="CommandException"/> to signal validation failure.
        /// </remarks>
        public abstract void Validate(T value);

        void IArgumentValidator.Validate(object? value) => Validate((T) value!);
    }
}
