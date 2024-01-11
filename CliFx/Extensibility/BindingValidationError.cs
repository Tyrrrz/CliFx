namespace CliFx.Extensibility;

/// <summary>
/// Represents a validation error.
/// </summary>
public class BindingValidationError(string message)
{
    /// <summary>
    /// Error message shown to the user.
    /// </summary>
    public string Message { get; } = message;
}
