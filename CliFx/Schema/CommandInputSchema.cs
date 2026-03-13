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

    internal object? Convert(IReadOnlyList<string> rawValues)
    {
        try
        {
            if (IsSequence)
            {
                // Sequence input with a sequence converter
                if (SequenceConverter is not null)
                    return SequenceConverter.ConvertMany(rawValues);

                // Sequence input without a sequence converter
                throw CliFxException.InternalError(
                    $"""
                     {this.GetKind()} {this.GetFormattedIdentifier()} is a sequence property but has no collection converter.
                     To fix this, use the source generator or provide a custom {nameof(
                         ISequenceBindingConverter
                     )} via the Converter attribute property.
                     """
                );
            }

            // Regular input
            if (rawValues.Count <= 1)
            {
                var rawValue = rawValues.SingleOrDefault();
                return Converter is not null ? Converter.Convert(rawValue) : rawValue;
            }
        }
        catch (Exception ex) when (ex is not CliFxException)
        {
            var errorMessage = ex is TargetInvocationException invokeEx
                ? invokeEx.InnerException?.Message ?? invokeEx.Message
                : ex.Message;

            throw CliFxException.UserError(
                $"""
                {this.GetKind()} {this.GetFormattedIdentifier()} cannot be set from the provided argument(s):
                {rawValues.Select(v => '<' + v + '>').JoinToString(" ")}
                Error: {errorMessage}
                """,
                ex
            );
        }

        // Mismatch (scalar but too many values)
        throw CliFxException.UserError(
            $"""
            {this.GetKind()} {this.GetFormattedIdentifier()} expects a single argument, but provided with multiple:
            {rawValues.Select(v => '<' + v + '>').JoinToString(" ")}
            """
        );
    }

    internal void Validate(object? convertedValue)
    {
        var errors = new List<BindingValidationError>();

        foreach (var validator in Validators)
        {
            var error = validator.Validate(convertedValue);
            if (error is not null)
                errors.Add(error);
        }

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
    }

    internal void Bind(IReadOnlyList<string> rawValues, ICommand instance)
    {
        var convertedValue = Convert(rawValues);
        Validate(convertedValue);

        try
        {
            Property.SetValue(instance, convertedValue);
        }
        catch (Exception ex) when (ex is not CliFxException)
        {
            var errorMessage = ex is TargetInvocationException invokeEx
                ? invokeEx.InnerException?.Message ?? ex.Message
                : ex.Message;

            throw CliFxException.UserError(
                $"""
                {this.GetKind()} {this.GetFormattedIdentifier()} cannot be set from the provided argument(s):
                {rawValues.Select(v => '<' + v + '>').JoinToString(" ")}
                Error: {errorMessage}
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
