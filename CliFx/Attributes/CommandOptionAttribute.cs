using System;
using CliFx.Extensibility;

namespace CliFx.Attributes;

/// <summary>
/// Binds a property to a command option — a command-line input that is identified by a name and/or a short name.
/// </summary>
/// <remarks>
/// All options in a command must have unique names (comparison IS NOT case-sensitive) and short names (comparison IS case-sensitive).
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class CommandOptionAttribute : CommandInputAttribute
{
    /// <summary>
    /// Initializes an instance of <see cref="CommandOptionAttribute" />.
    /// </summary>
    private CommandOptionAttribute(string? name, char? shortName)
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
    /// Option name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Option short name.
    /// </summary>
    public char? ShortName { get; }

    /// <summary>
    /// Whether this option is required (default: <c>false</c>).
    /// If an option is required, the user will get an error if they don't set it.
    /// </summary>
    /// <remarks>
    /// You can use the <c>required</c> keyword on the property (introduced in C# 11) to implicitly
    /// set <see cref="IsRequired" /> to <c>true</c>.
    /// </remarks>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Environment variable whose value will be used as a fallback if the option
    /// has not been explicitly set through command-line arguments.
    /// </summary>
    public string? EnvironmentVariable { get; set; }
}
