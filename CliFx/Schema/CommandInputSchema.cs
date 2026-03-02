using System;
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
    /// </summary>
    public IBindingConverter? Converter { get; } = converter;

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
    IReadOnlyList<BindingValidator<TProperty>> validators
) : CommandInputSchema(property, isSequence, description, converter, validators)
    where TCommand : ICommand;

internal static class CommandInputSchemaExtensions
{
    public static string GetKind(this CommandInputSchema schema) =>
        schema switch
        {
            CommandParameterSchema => "Parameter",
            CommandOptionSchema => "Option",
            _ => throw new InvalidOperationException(
                $"Unknown input schema type: '{schema.GetType().Name}'."
            ),
        };

    public static string GetFormattedIdentifier(this CommandInputSchema schema) =>
        schema switch
        {
            CommandParameterSchema parameter => parameter.IsSequence
                ? $"<{parameter.Name}...>"
                : $"<{parameter.Name}>",

            CommandOptionSchema option => option switch
            {
                { Name: not null, ShortName: not null } => $"-{option.ShortName}|--{option.Name}",
                { Name: not null } => $"--{option.Name}",
                { ShortName: not null } => $"-{option.ShortName}",
                _ => throw new InvalidOperationException("Option must have a name or short name."),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(schema)),
        };
}
