namespace CliFx.Activation;

/// <summary>
/// Represents a validation error for a command input.
/// </summary>
public class InputValidationError(string message)
{
    /// <summary>
    /// Error message shown to the user.
    /// </summary>
    public string Message { get; } = message;
}
