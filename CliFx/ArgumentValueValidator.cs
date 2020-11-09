namespace CliFx
{
    /// <summary>
    /// A base type for custom validators.
    /// </summary>
    public abstract class ArgumentValueValidator<T> : IArgumentValueValidator
    {
        /// <summary>
        /// Your validation logic have to be implemented in this method.
        /// </summary>
        public abstract ValidationResult Validate(T value);

        /// <summary>
        /// Non-generic method, will be called by the framework.
        /// </summary>
        public ValidationResult Validate(object value) => Validate((T) value);
    }
}
