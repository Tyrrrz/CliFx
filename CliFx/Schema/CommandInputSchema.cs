using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CliFx.Exceptions;
using CliFx.Infrastructure.Binding;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

/// <summary>
/// Describes an input (parameter or option) of a command.
/// </summary>
public abstract class CommandInputSchema(
    PropertyBinding property,
    bool isRequired,
    string? description,
    IBindingConverter converter,
    IReadOnlyList<IBindingValidator> validators
)
{
    /// <summary>
    /// CLR property to which this input is bound.
    /// </summary>
    public PropertyBinding Property { get; } = property;

    /// <summary>
    /// Whether this option must be provided.
    /// </summary>
    public bool IsRequired { get; } = isRequired;

    /// <summary>
    /// Whether this input accepts multiple values (sequence property).
    /// </summary>
    public bool IsSequence => Converter.IsSequence;

    /// <summary>
    /// Description shown in the help text.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Binding converter used to convert raw command-line argument(s) to the target property type.
    /// </summary>
    public IBindingConverter Converter { get; } = converter;

    /// <summary>
    /// Binding validators used to validate the converted value before setting it to the target property.
    /// </summary>
    public IReadOnlyList<IBindingValidator> Validators { get; } = validators;

    internal void Activate(ICommand instance, IReadOnlyList<string> rawValues)
    {
        var value = Converter.Convert(rawValues);

        var errors = Validators
            .Select(validator => validator.Validate(value))
            .OfType<BindingValidationError>()
            .ToArray();

        if (errors.Any())
        {
            throw CliFxException.UserError(
                $"""
                {this.GetKind()} {this.GetFormattedIdentifier()} has been provided with an invalid value.
                Error(s):
                {errors.Select(e => "- " + e.Message).JoinToString(Environment.NewLine)}
                """
            );
        }

        try
        {
            Property.SetValue(instance, value);
        }
        catch (Exception ex) when (ex is not CliFxException)
        {
            throw CliFxException.UserError(
                $"""
                {this.GetKind()} {this.GetFormattedIdentifier()} cannot be set from the provided argument(s):
                {rawValues.Select(v => '<' + v + '>').JoinToString(" ")}
                Error: {ex.Message}
                """,
                ex
            );
        }
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => this.GetFormattedIdentifier();
}

/// <inheritdoc cref="CommandInputSchema" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public abstract class CommandInputSchema<TCommand, TProperty>(
    PropertyBinding<TCommand, TProperty> property,
    bool isRequired,
    string? description,
    BindingConverter<TProperty> converter,
    IReadOnlyList<BindingValidator<TProperty>> validators
) : CommandInputSchema(property, isRequired, description, converter, validators)
    where TCommand : ICommand;
