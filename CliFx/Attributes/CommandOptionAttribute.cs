using System;
using CliFx.Extensibility;

namespace CliFx.Attributes;

/// <summary>
/// Annotates a property that defines a command option.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class CommandOptionAttribute : Attribute
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
    /// <remarks>
    /// Must contain at least two characters and start with a letter.
    /// Either <see cref="Name" /> or <see cref="ShortName" /> must be set.
    /// All options in a command must have unique names (comparison IS NOT case-sensitive).
    /// </remarks>
    public string? Name { get; }

    /// <summary>
    /// Option short name.
    /// </summary>
    /// <remarks>
    /// Either <see cref="Name" /> or <see cref="ShortName" /> must be set.
    /// All options in a command must have unique short names (comparison IS case-sensitive).
    /// </remarks>
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

    /// <summary>
    /// Option description.
    /// This is shown to the user in the help text.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Custom converter used for mapping the raw command-line argument into
    /// a value expected by the underlying property.
    /// </summary>
    /// <remarks>
    /// Converter must derive from <see cref="BindingConverter{T}" />.
    /// </remarks>
    public Type? Converter { get; set; }

    /// <summary>
    /// Custom validators used for verifying the value of the underlying
    /// property, after it has been bound.
    /// </summary>
    /// <remarks>
    /// Validators must derive from <see cref="BindingValidator{T}" />.
    /// </remarks>
    public Type[] Validators { get; set; } = Array.Empty<Type>();
}
