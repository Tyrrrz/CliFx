using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CliFx.Infrastructure.Binding;

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
    IBindingConverter? converter,
    ISequenceBindingConverter? sequenceConverter,
    IReadOnlyList<IBindingValidator> validators
) : CommandInputSchema(property, isSequence, description, converter, sequenceConverter, validators)
{
    /// <summary>
    /// Position order of this parameter.
    /// </summary>
    public int Order { get; } = order;

    /// <summary>
    /// Parameter name, shown in the help text.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Whether this parameter must be provided.
    /// </summary>
    public bool IsRequired { get; } = isRequired;
}

/// <inheritdoc cref="CommandParameterSchema" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public class CommandParameterSchema<
    TCommand,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TProperty
>(
    PropertyBinding<TCommand, TProperty> property,
    bool isSequence,
    int order,
    string name,
    bool isRequired,
    string? description,
    BindingConverter<TProperty>? converter,
    SequenceBindingConverter<TProperty>? sequenceConverter,
    IReadOnlyList<IBindingValidator> validators
)
    : CommandParameterSchema(
        property,
        isSequence,
        order,
        name,
        isRequired,
        description,
        converter,
        sequenceConverter,
        validators
    )
    where TCommand : ICommand;
