using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Extensibility;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

/// <summary>
/// Describes an input of a command.
/// </summary>
public abstract class CommandInputSchema(
    PropertyBinding property,
    string? description,
    IBindingConverter converter,
    IReadOnlyList<IBindingValidator> validators
)
{
    internal abstract string Kind { get; }

    internal abstract string FormattedIdentifier { get; }

    internal bool IsSequence { get; } =
        property.Type != typeof(string)
        && property.Type.TryGetEnumerableUnderlyingType() is not null;

    /// <summary>
    /// Input description, used in the help text.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// CLR property to which this input is bound.
    /// </summary>
    public PropertyBinding Property { get; } = property;

    /// <summary>
    /// Binding converter used for this input.
    /// </summary>
    public IBindingConverter Converter { get; } = converter;

    /// <summary>
    /// Binding validator(s) used for this input.
    /// </summary>
    public IReadOnlyList<IBindingValidator> Validators { get; } = validators;

    private void Validate(object? value)
    {
        var errors = Validators
            .Select(validator => validator.Validate(value))
            .OfType<BindingValidationError>()
            .ToArray();

        if (errors.Any())
        {
            throw CliFxException.UserError(
                $"""
                {Kind} {FormattedIdentifier} has been provided with an invalid value.
                Error(s):
                {errors.Select(e => "- " + e.Message).JoinToString(Environment.NewLine)}
                """
            );
        }
    }

    internal void Activate(ICommand instance, IReadOnlyList<string?> rawInputs)
    {
        var formatProvider = CultureInfo.InvariantCulture;

        try
        {
            // Multiple values expected, single or multiple values provided
            if (IsSequence)
            {
                var value = rawInputs.Select(v => Converter.Convert(v, formatProvider)).ToArray();
                Validate(value);

                Property.SetValue(instance, value);
            }
            // Single value expected, single value provided
            else if (rawInputs.Count <= 1)
            {
                var value = Converter.Convert(rawInputs.SingleOrDefault(), formatProvider);
                Validate(value);

                Property.SetValue(instance, value);
            }
            // Single value expected, multiple values provided
            else
            {
                throw CliFxException.UserError(
                    $"""
                     {Kind} {FormattedIdentifier} expects a single argument, but provided with multiple:
                     {rawInputs.Select(v => '<' + v + '>').JoinToString(" ")}
                     """
                );
            }
        }
        catch (Exception ex) when (ex is not CliFxException) // don't wrap CliFxException
        {
            throw CliFxException.UserError(
                $"""
                 {Kind} {FormattedIdentifier} cannot be set from the provided argument(s):
                 {rawInputs.Select(v => '<' + v + '>').JoinToString(" ")}
                 Error: {ex.Message}
                 """,
                ex
            );
        }
    }

    /// <inheritdoc />
    public override string ToString() => FormattedIdentifier;
}

/// <inheritdoc cref="CommandInputSchema" />
/// <remarks>
/// Generic version of the type is used to simplify initialization from source-generated code and
/// to enforce static references to all types used in the binding.
/// The non-generic version is used internally by the framework when operating in a dynamic context.
/// </remarks>
public abstract class CommandInputSchema<
    TCommand,
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicMethods
    )]
        TProperty
>(
    PropertyBinding<TCommand, TProperty> property,
    string? description,
    BindingConverter<TProperty> converter,
    IReadOnlyList<BindingValidator<TProperty>> validators
) : CommandInputSchema(property, description, converter, validators)
    where TCommand : ICommand;
