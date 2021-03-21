namespace CliFx.Extensibility
{
    /// <summary>
    /// Represents a validation error.
    /// </summary>
    public class BindingValidationError
    {
        /// <summary>
        /// Error message shown to the user.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes an instance of <see cref="BindingValidationError"/>.
        /// </summary>
        public BindingValidationError(string message) => Message = message;
    }
}