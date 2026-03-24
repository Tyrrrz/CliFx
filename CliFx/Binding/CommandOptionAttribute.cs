using System;

namespace CliFx.Binding;

/// <summary>
/// Binds a property to an option input of a command.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CommandOptionAttribute : CommandInputAttribute
{
    /// <summary>
    /// Initializes an instance of <see cref="CommandOptionAttribute" />.
    /// </summary>
    protected CommandOptionAttribute(string? name, char? shortName)
    {
        Name = name;
        ShortName = shortName;
    }

    /// <summary>
    /// Initializes an instance of <see cref="CommandOptionAttribute" />.
    /// </summary>
    public CommandOptionAttribute(string name, char shortName)
        : this(name, (char?)shortName) { }

    /// <summary>
    /// Initializes an instance of <see cref="CommandOptionAttribute" />.
    /// </summary>
    public CommandOptionAttribute(string name)
        : this(name, null) { }

    /// <summary>
    /// Initializes an instance of <see cref="CommandOptionAttribute" />.
    /// </summary>
    public CommandOptionAttribute(char shortName)
        : this(null, (char?)shortName) { }

    /// <summary>
    /// Option name (i.e. the --name form).
    /// </summary>
    /// <remarks>
    /// Must contain at least two characters and start with a letter.
    /// Either <see cref="Name" /> or <see cref="ShortName" /> must be set.
    /// All options in a command must have unique names (comparison IS NOT case-sensitive).
    /// </remarks>
    public string? Name { get; }

    /// <summary>
    /// Option short name (i.e. the -n form).
    /// </summary>
    /// <remarks>
    /// Either <see cref="Name" /> or <see cref="ShortName" /> must be set.
    /// All options in a command must have unique short names (comparison IS case-sensitive).
    /// </remarks>
    public char? ShortName { get; }

    /// <summary>
    /// Environment variable whose value will be used as a fallback if the option
    /// has not been explicitly set through command-line arguments.
    /// </summary>
    public string? EnvironmentVariable { get; set; }
}
