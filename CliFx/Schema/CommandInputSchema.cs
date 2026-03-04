using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes an input (parameter or option) of a command.
/// </summary>
public abstract class CommandInputSchema(
    PropertyBinding property,
    bool isSequence,
    string? description,
    IBindingConverter? converter,
    ISequenceBindingConverter? sequenceConverter,
    IReadOnlyList<IBindingValidator> validators
)
{
    /// <summary>
    /// CLR property to which this input is bound.
    /// </summary>
    public PropertyBinding Property { get; } = property;

    /// <summary>
    /// Whether this input accepts multiple values (sequence property).
    /// </summary>
    public bool IsSequence { get; } = isSequence;

    /// <summary>
    /// Description shown in the help text.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Binding converter used to parse raw strings into the property type.
    /// For sequence properties, <see cref="SequenceConverter"/> takes precedence when set.
    /// </summary>
    public IBindingConverter? Converter { get; } = converter;

    /// <summary>
    /// Binding converter used to parse multiple raw strings into the collection property type.
    /// When set on a sequence property, this is used instead of per-element <see cref="Converter"/>.
    /// </summary>
    public ISequenceBindingConverter? SequenceConverter { get; } = sequenceConverter;

    /// <summary>
    /// Validators run after the value is converted.
    /// </summary>
    public IReadOnlyList<IBindingValidator> Validators { get; } = validators;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => this.GetFormattedIdentifier();
}

/// <inheritdoc cref="CommandInputSchema" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public abstract class CommandInputSchema<
    TCommand,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TProperty
>(
    PropertyBinding<TCommand, TProperty> property,
    bool isSequence,
    string? description,
    BindingConverter<TProperty>? converter,
    SequenceBindingConverter<TProperty>? sequenceConverter,
    IReadOnlyList<IBindingValidator> validators
) : CommandInputSchema(property, isSequence, description, converter, sequenceConverter, validators)
    where TCommand : ICommand;
