﻿using System;

namespace CliFx.Attributes;

/// <summary>
/// Annotates a type that defines a command.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// Initializes an instance of <see cref="CommandAttribute" />.
    /// </summary>
    public CommandAttribute(string name) => Name = name;

    /// <summary>
    /// Initializes an instance of <see cref="CommandAttribute" />.
    /// </summary>
    public CommandAttribute() { }

    /// <summary>
    /// Command name.
    /// </summary>
    /// <remarks>
    /// Command can have no name, in which case it's treated as the application's default command.
    /// Only one default command is allowed in an application.
    /// All commands registered in an application must have unique names (comparison IS NOT case-sensitive).
    /// </remarks>
    public string? Name { get; }

    /// <summary>
    /// Command description.
    /// This is shown to the user in the help text.
    /// </summary>
    public string? Description { get; set; }
}
