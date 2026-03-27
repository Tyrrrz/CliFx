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
    /// If left unset, the default conversion logic will be used.
    /// </para>
    /// <para>
    /// Converters must derive from <see cref="IInputConverter" />.
    /// </para>
    /// <para>
    /// To implement your own converter, inherit from <see cref="ScalarInputConverter{T}" /> for
    /// scalar (single-value) inputs and from <see cref="SequenceInputConverter{T}" /> for
    /// sequence-based (multi-value) inputs.
    /// </para>
    /// </remarks>
    public Type? Converter { get; set; }

    /// <summary>
    /// Whether the specified <see cref="Converter" /> is an element converter that should be
    /// automatically wrapped in an appropriate sequence converter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When set to <c>true</c>, the provided converter must be a scalar converter
    /// (derived from <see cref="ScalarInputConverter{T}" />) that converts individual elements
    /// of the sequence. CliFx will automatically wrap it in a sequence converter that applies
    /// the element converter to each argument.
    /// </para>
    /// <para>
    /// This property cannot be used with a sequence converter.
    /// </para>
    /// </remarks>
    public bool IsElementConverter { get; set; }

    /// <summary>
    /// Custom validator(s) used for verifying the value of this input after activation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Validators must derive from <see cref="IInputValidator" />.
    /// </para>
    /// <para>
    /// To implement your own validator, inherit from <see cref="InputValidator{T}" />.
    /// </para>
    /// </remarks>
    public Type[] Validators { get; set; } = [];
}
