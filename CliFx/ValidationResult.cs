namespace CliFx
{
    /// <summary>
    /// A tiny object that represents a result of the validation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// False if there is no error message, otherwise - true.
        /// </summary>
        public bool IsValid => ErrorMessage == null;

        /// <summary>
        /// Contains an information about the reasons of failed validation.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        private ValidationResult() { }

        /// <summary>
        /// Creates Ok result, means that the validation is passed.
        /// </summary>
        public static ValidationResult Ok() => new ValidationResult() { };

        /// <summary>
        /// Creates Error result, means that the validation failed.
        /// </summary>
        public static ValidationResult Error(string message) => new ValidationResult() { ErrorMessage = message }; 
    }
}
