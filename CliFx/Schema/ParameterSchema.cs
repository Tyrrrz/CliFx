using System.Collections.Generic;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes a command's parameter.
/// </summary>
public class ParameterSchema(
    PropertyBinding property,
    bool isScalar,
    IReadOnlyList<object?>? validValues,
    int order,
    string name,
    bool isRequired,
    string? description,
    IBindingConverter? converter,
    IReadOnlyList<IBindingValidator> validators
) : IInputSchema
{
    /// <inheritdoc />
    public PropertyBinding Property { get; } = property;

    /// <inheritdoc />
    public bool IsScalar { get; } = isScalar;

    /// <inheritdoc />
    public IReadOnlyList<object?>? ValidValues { get; } = validValues;

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

    /// <inheritdoc />
    public IBindingConverter? Converter { get; } = converter;

    /// <inheritdoc />
    public IReadOnlyList<IBindingValidator> Validators { get; } = validators;

    internal string GetFormattedIdentifier() =>
        IsScalar ? $"<{Name}>" : $"<{Name}...>";
}
