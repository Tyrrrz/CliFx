using System;
using CliFx.Activation;

namespace CliFx.Binding;

/// <summary>
/// Binds a property to an input (parameter or option) of a command.
/// </summary>
public abstract class CommandInputAttribute : Attribute
{
    /// <summary>
    /// Description of the input.
    /// Used for display purposes in the help text.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Custom converter used for activating this input from raw command-line arguments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If left empty, the default conversion logic will be used.
    /// </para>
    /// <para>
    /// Converter must derive from <see cref="IInputConverter" />.
    /// </para>
    /// </remarks>
    public Type? Converter { get; set; }

    /// <summary>
    /// Custom validator(s) used for verifying the value of this input after activation.
    /// </summary>
    /// <remarks>
    /// Validators must derive from <see cref="IInputValidator" />.
    /// </remarks>
    public Type[] Validators { get; set; } = [];
}
