using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes a parameter input of a command.
/// </summary>
public class CommandParameterSchema(
    PropertyBinding property,
    bool isSequence,
    int order,
    string name,
    bool isRequired,
    string? description,
    IBindingConverter converter,
    IReadOnlyList<IBindingValidator> validators
) : CommandInputSchema(property, isSequence, description, converter, validators)
{
    /// <summary>
    /// Order, in which the parameter is activated from the command-line arguments.
    /// </summary>
    public int Order { get; } = order;

    /// <summary>
    /// Parameter name, used in the help text.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Whether the parameter is required.
    /// </summary>
    public bool IsRequired { get; } = isRequired;
}

/// <inheritdoc cref="CommandParameterSchema" />
/// <remarks>
/// Generic version of the type is used to simplify initialization from source-generated code and
/// to enforce static references to all types used in the binding.
/// The non-generic version is used internally by the framework when operating in a dynamic context.
/// </remarks>
public class CommandParameterSchema<
    TCommand,
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods
    )]
        TProperty
>(
    PropertyBinding<TCommand, TProperty> property,
    bool isSequence,
    int order,
    string name,
    bool isRequired,
    string? description,
    BindingConverter<TProperty> converter,
    IReadOnlyList<BindingValidator<TProperty>> validators
)
    : CommandParameterSchema(
        property,
        isSequence,
        order,
        name,
        isRequired,
        description,
        converter,
        validators
    )
    where TCommand : ICommand;
