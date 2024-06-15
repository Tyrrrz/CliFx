using System.Collections.Generic;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes a parameter binding of a command.
/// </summary>
public class ParameterSchema(
    PropertyBinding property,
    bool isSequence,
    int order,
    string name,
    bool isRequired,
    string? description,
    IBindingConverter? converter,
    IReadOnlyList<IBindingValidator> validators
) : InputSchema(property, isSequence, converter, validators)
{
    /// <summary>
    /// Order, in which the parameter is bound from the command-line arguments.
    /// </summary>
    public int Order { get; } = order;

    /// <summary>
    /// Parameter name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Whether the parameter is required.
    /// </summary>
    public bool IsRequired { get; } = isRequired;

    /// <summary>
    /// Parameter description.
    /// </summary>
    public string? Description { get; } = description;

    internal string GetFormattedIdentifier() => IsSequence ? $"<{Name}>" : $"<{Name}...>";
}
