namespace CliFx.Attributes;

/// <summary>
/// Binds a property to the version option of a command.
/// </summary>
/// <remarks>
/// This attribute is applied automatically by the framework and should not need to be used explicitly.
/// </remarks>
public class CommandVersionOptionAttribute : CommandOptionAttribute
{
    /// <summary>
    /// Initializes an instance of <see cref="CommandVersionOptionAttribute" />.
    /// </summary>
    public CommandVersionOptionAttribute()
        : base("version")
    {
        Description = "Show application version.";
    }
}
