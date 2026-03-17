using System;

namespace CliFx.Binding;

/// <summary>
/// Binds a class to a command.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class CommandAttribute(string? name) : Attribute
{
    /// <summary>
    /// Initializes an instance of <see cref="CommandAttribute" />.
    /// </summary>
    public CommandAttribute()
        : this(null) { }

    /// <summary>
    /// Command name.
    /// </summary>
    /// <remarks>
    /// Command can have no name, in which case it's treated as the application's default command.
    /// Only one default command is allowed to be registered in an application.
    /// All commands registered in an application must have unique names (comparison IS NOT case-sensitive).
    /// </remarks>
    public string? Name { get; } = name;

    /// <summary>
    /// Command description.
    /// Used for display purposes in the help text.
    /// </summary>
    public string? Description { get; set; }
}
