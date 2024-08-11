using System;
using CliFx.Extensibility;

namespace CliFx.Attributes;

/// <summary>
/// Binds a property to a command-line input.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class CommandInputAttribute : Attribute
{
    /// <summary>
    /// Input description, as shown in the help text.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Custom converter used for mapping the raw command-line argument into
    /// the type and shape expected by the underlying property.
    /// </summary>
    /// <remarks>
    /// Converter must derive from <see cref="BindingConverter{T}" />.
    /// </remarks>
    public Type? Converter { get; set; }

    /// <summary>
    /// Custom validators used for verifying the value of the underlying
    /// property, after it has been set.
    /// </summary>
    /// <remarks>
    /// Validators must derive from <see cref="BindingValidator{T}" />.
    /// </remarks>
    public Type[] Validators { get; set; } = [];
}