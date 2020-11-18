namespace CliFx
{
    /// <summary>
    /// Represents a result of a validation.
    /// </summary>
    public partial class ValidationResult
    {
        /// <summary>
        /// Whether validation was successful.
        /// </summary>
        public bool IsValid => ErrorMessage == null;

        /// <summary>
        /// If validation has failed, contains the associated error, otherwise null.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ValidationResult"/>.
        /// </summary>
        public ValidationResult(string? errorMessage = null) =>
            ErrorMessage = errorMessage;
    }

    public partial class ValidationResult
    {
        /// <summary>
        /// Creates successful result, meaning that the validation has passed.
        /// </summary>
        public static ValidationResult Ok() => new ValidationResult();

        /// <summary>
        /// Creates an error result, meaning that the validation has failed.
        /// </summary>
        public static ValidationResult Error(string message) => new ValidationResult(message);
    }

    internal interface IArgumentValueValidator
    {
        ValidationResult Validate(object value);
    }

    /// <summary>
    /// A base type for custom argument validators.
    /// </summary>
    public abstract class ArgumentValueValidator<T> : IArgumentValueValidator
    {
        /// <summary>
        /// Validates the value.
        /// </summary>
        public abstract ValidationResult Validate(T value);

        ValidationResult IArgumentValueValidator.Validate(object value) => Validate((T) value);
    }
}
