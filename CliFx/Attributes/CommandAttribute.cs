using System;

namespace CliFx.Attributes;

/// <summary>
/// Annotates a type that defines a command.
/// If a command is named, then the user must provide its name through the command-line arguments in order to execute it.
/// If a command is not named, then it is treated as the application's default command and is executed when no other
/// command is specified.
/// </summary>
/// <remarks>
/// Only one default command is allowed per application.
/// All commands registered in an application must have unique names (comparison IS NOT case-sensitive).
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CommandAttribute(string? name = null) : Attribute
{
    /// <summary>
    /// Command name.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Command description.
    /// This is shown to the user in the help text.
    /// </summary>
    public string? Description { get; set; }
}