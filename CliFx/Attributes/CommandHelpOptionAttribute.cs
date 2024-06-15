namespace CliFx.Attributes;

/// <summary>
/// Annotates a property that defines the help option for a command.
/// </summary>
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
