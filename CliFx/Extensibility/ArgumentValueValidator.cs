namespace CliFx.Extensibility
{
    // Used internally to simplify usage from reflection
    internal interface IArgumentValueValidator
    {
        ValidationResult Validate(object value);
    }

    /// <summary>
    /// Base type for custom argument validators.
    /// </summary>
    public abstract class ArgumentValueValidator<T> : IArgumentValueValidator
    {
        /// <summary>
        /// Validates the value.
        /// </summary>
        public abstract ValidationResult Validate(T value);

        ValidationResult IArgumentValueValidator.Validate(object value) => Validate((T) value);
    }

    /// <summary>
    /// Represents the result of a validation.
    /// </summary>
    public partial class ValidationResult
    {
        /// <summary>
        /// If validation has failed, contains the associated error, otherwise null.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Whether validation was successful.
        /// </summary>
        public bool IsValid => ErrorMessage is null;

        /// <summary>
        /// Initializes an instance of <see cref="ValidationResult"/>.
        /// </summary>
        public ValidationResult(string? errorMessage = null) =>
            ErrorMessage = errorMessage;
    }

    public partial class ValidationResult
    {
        /// <summary>
        /// Creates a successful result, meaning that the validation has passed.
        /// </summary>
        public static ValidationResult Ok() => new();

        /// <summary>
        /// Creates an unsuccessful result, meaning that the validation has failed.
        /// The provided error message will be printed to the user for feedback.
        /// </summary>
        public static ValidationResult Error(string message) => new(message);
    }
}
