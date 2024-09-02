namespace CliFx.Attributes;

/// <summary>
/// Binds a property to the help option of a command.
/// </summary>
/// <remarks>
/// This attribute is applied automatically by the framework and should not need to be used explicitly.
/// </remarks>
public class CommandHelpOptionAttribute : CommandOptionAttribute
{
    /// <summary>
    /// Initializes an instance of <see cref="CommandHelpOptionAttribute" />.
    /// </summary>
    public CommandHelpOptionAttribute()
        : base("help", 'h')
    {
        Description = "Show help for this command.";
    }
}
