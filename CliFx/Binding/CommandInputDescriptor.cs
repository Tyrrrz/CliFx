using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CliFx.Activation;

namespace CliFx.Binding;

/// <summary>
/// Describes an input (parameter or option) of a command.
/// </summary>
public abstract class CommandInputDescriptor(
    PropertyDescriptor property,
    bool isRequired,
    string? description,
    IInputConverter converter,
    IReadOnlyList<IInputValidator> validators
)
{
    /// <summary>
    /// CLR property to which this input is bound.
    /// </summary>
    public PropertyDescriptor Property { get; } = property;

    /// <summary>
    /// Whether this input must always be provided.
    /// </summary>
    public bool IsRequired { get; } = isRequired;

    /// <inheritdoc cref="CommandInputAttribute.Description" />
    public string? Description { get; } = description;

    /// <summary>
    /// Converter used to convert raw command-line argument(s) to the target property type.
    /// </summary>
    public IInputConverter Converter { get; } = converter;

    /// <summary>
    /// Validators used to validate the converted value before setting it to the target property.
    /// </summary>
    public IReadOnlyList<IInputValidator> Validators { get; } = validators;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => this.GetFormattedIdentifier();
}

/// <inheritdoc cref="CommandInputDescriptor" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public abstract class CommandInputDescriptor<TCommand, TProperty>(
    PropertyDescriptor<TCommand, TProperty> property,
    bool isRequired,
    string? description,
    InputConverter<TProperty> converter,
    IReadOnlyList<InputValidator<TProperty>> validators
) : CommandInputDescriptor(property, isRequired, description, converter, validators)
    where TCommand : ICommand;
