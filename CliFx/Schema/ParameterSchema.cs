using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes a parameter input of a command.
/// </summary>
public class ParameterSchema(
    PropertyBinding property,
    int order,
    string name,
    bool isRequired,
    string? description,
    IBindingConverter converter,
    IReadOnlyList<IBindingValidator> validators
) : InputSchema(property, converter, validators)
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

// Generic version of the type is used to simplify initialization from the source-generated code
// and to enforce static references to all the types used in the binding.
// The non-generic version is used internally by the framework when operating in a dynamic context.
/// <inheritdoc cref="ParameterSchema" />
public class ParameterSchema<
    TCommand,
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicMethods
    )]
        TProperty
>(
    PropertyBinding<TCommand, TProperty> property,
    int order,
    string name,
    bool isRequired,
    string? description,
    BindingConverter<TProperty> converter,
    IReadOnlyList<BindingValidator<TProperty>> validators
) : ParameterSchema(property, order, name, isRequired, description, converter, validators)
    where TCommand : ICommand;
