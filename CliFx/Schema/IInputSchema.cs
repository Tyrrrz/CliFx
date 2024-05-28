using System.Collections.Generic;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes an input of a command, which can be either a parameter or an option.
/// </summary>
public interface IInputSchema
{
    /// <summary>
    /// Information about the property that this input is bound to.
    /// </summary>
    PropertyDescriptor Property { get; }

    /// <summary>
    /// Whether this input is a scalar (single value) or a sequence (multiple values).
    /// </summary>
    bool IsScalar { get; }

    /// <summary>
    /// Valid values for this input, if applicable.
    /// If the input does not have a predefined set of valid values, this property is <c>null</c>.
    /// </summary>
    IReadOnlyList<object?>? ValidValues { get; }

    /// <summary>
    /// Optional binding converter for this input.
    /// </summary>
    IBindingConverter? Converter { get; }

    /// <summary>
    /// Optional binding validator(s) for this input.
    /// </summary>
    IReadOnlyList<IBindingValidator> Validators { get; }
}
