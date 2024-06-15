namespace CliFx.Attributes;

/// <summary>
/// Annotates a property that defines the version option for a command.
/// </summary>
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
