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
public abstract class InputSchema(
    PropertyBinding property,
    IBindingConverter converter,
    IReadOnlyList<IBindingValidator> validators
)
{
    internal bool IsSequence { get; } =
        property.Type != typeof(string)
        && property.Type.TryGetEnumerableUnderlyingType() is not null;

    /// <summary>
    /// CLR property to which this input is bound.
    /// </summary>
    public PropertyBinding Property { get; } = property;

    /// <summary>
    /// Optional binding converter for this input.
    /// </summary>
    public IBindingConverter Converter { get; } = converter;

    /// <summary>
    /// Optional binding validator(s) for this input.
    /// </summary>
    public IReadOnlyList<IBindingValidator> Validators { get; } = validators;

    internal void Validate(object? value)
    {
        var errors = new List<BindingValidationError>();

        foreach (var validator in validators)
        {
            var error = validator.Validate(value);

            if (error is not null)
                errors.Add(error);
        }

        if (errors.Any())
        {
            throw CliFxException.UserError(
                $"""
                {memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} has been provided with an invalid value.
                Error(s):
                {errors.Select(e => "- " + e.Message).JoinToString(Environment.NewLine)}
                """
            );
        }
    }

    internal void Set(ICommand command, IReadOnlyList<string?> rawInputs)
    {
        var formatProvider = CultureInfo.InvariantCulture;

        // Multiple values expected, single or multiple values provided
        if (IsSequence)
        {
            var value = rawInputs.Select(v => Converter.Convert(v, formatProvider)).ToArray();
            Validate(value);

            Property.Set(command, value);
        }
        // Single value expected, single value provided
        else if (rawInputs.Count <= 1)
        {
            var value = Converter.Convert(rawInputs.SingleOrDefault(), formatProvider);
            Validate(value);

            Property.Set(command, value);
        }
        // Single value expected, multiple values provided
        else
        {
            throw CliFxException.UserError(
                $"""
                {memberSchema.GetKind()} {memberSchema.GetFormattedIdentifier()} expects a single argument, but provided with multiple:
                {rawInputs.Select(v => '<' + v + '>').JoinToString(" ")}
                """
            );
        }
    }
}

// Generic version of the type is used to simplify initialization from the source-generated code
// and to enforce static references to all the types used in the binding.
// The non-generic version is used internally by the framework when operating in a dynamic context.
/// <inheritdoc cref="InputSchema" />
public abstract class InputSchema<
    TCommand,
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicMethods
    )]
        TProperty
>(
    PropertyBinding<TCommand, TProperty> property,
    BindingConverter<TProperty> converter,
    IReadOnlyList<BindingValidator<TProperty>> validators
) : InputSchema(property, converter, validators)
    where TCommand : ICommand;
