using System.Collections.Generic;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes an input of a command.
/// </summary>
public abstract class InputSchema(
    PropertyBinding property,
    bool isSequence,
    IBindingConverter? converter,
    IReadOnlyList<IBindingValidator> validators
)
{
    /// <summary>
    /// CLR property to which this input is bound.
    /// </summary>
    public PropertyBinding Property { get; } = property;

    /// <summary>
    /// Whether the input can accept more than one value.
    /// </summary>
    public bool IsSequence { get; } = isSequence;

    /// <summary>
    /// Optional binding converter for this input.
    /// </summary>
    public IBindingConverter? Converter { get; } = converter;

    /// <summary>
    /// Optional binding validator(s) for this input.
    /// </summary>
    public IReadOnlyList<IBindingValidator> Validators { get; } = validators;
}
