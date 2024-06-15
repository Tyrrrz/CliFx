using System.Collections.Generic;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes an input of a command, which can be either a parameter or an option.
/// </summary>
public interface IInputSchema
{
    /// <summary>
    /// Describes the binding of this input to a CLR property.
    /// </summary>
    PropertyBinding Property { get; }

    /// <summary>
    /// Optional binding converter for this input.
    /// </summary>
    IBindingConverter? Converter { get; }

    /// <summary>
    /// Optional binding validator(s) for this input.
    /// </summary>
    IReadOnlyList<IBindingValidator> Validators { get; }
}
