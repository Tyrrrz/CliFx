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
    bool isSequence,
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
    /// Whether this input is a sequence (i.e. multiple values can be provided).
    /// </summary>
    public bool IsSequence { get; } = isSequence;

    /// <summary>
    /// Input description, used in the help text.
    /// </summary>
    public string? Description { get; } = description;

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
            .WhereNotNull()
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
    }

    internal void Activate(ICommand instance, IReadOnlyList<string?> rawValues)
    {
        var formatProvider = CultureInfo.InvariantCulture;

        try
        {
            // Sequential input; zero or more values provided
            if (IsSequence)
            {
                var values = rawValues.Select(v => Converter.Convert(v, formatProvider)).ToArray();

                // TODO: cast array to the proper type

                Validate(values);
                Property.SetValue(instance, values);
            }
            // Non-sequential input; zero or one value provided
            else if (rawValues.Count <= 1)
            {
                var value = Converter.Convert(rawValues.SingleOrDefault(), formatProvider);

                Validate(value);
                Property.SetValue(instance, value);
            }
            // Non-sequential input; more than one value provided
            else
            {
                throw CliFxException.UserError(
                    $"""
                    {this.GetKind()} {this.GetFormattedIdentifier()} expects a single value, but provided with multiple:
                    {rawValues.Select(v => '<' + v + '>').JoinToString(" ")}
                    """
                );
            }
        }
        catch (Exception ex) when (ex is not CliFxException) // don't wrap CliFxException
        {
            throw CliFxException.UserError(
                $"""
                {this.GetKind()} {this.GetFormattedIdentifier()} cannot be set from the provided value(s):
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
/// Generic version of the type is used to simplify initialization from source-generated code and
/// to enforce static references to all types used in the binding.
/// The non-generic version is used internally by the framework when operating in a dynamic context.
/// </remarks>
public abstract class CommandInputSchema<
    TCommand,
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods
    )]
        TProperty
>(
    PropertyBinding<TCommand, TProperty> property,
    bool isSequence,
    string? description,
    BindingConverter<TProperty> converter,
    IReadOnlyList<BindingValidator<TProperty>> validators
) : CommandInputSchema(property, isSequence, description, converter, validators)
    where TCommand : ICommand;

// Define these as extension methods to avoid exposing them as protected members (i.e. essentially public API)
internal static class CommandInputSchemaExtensions
{
    public static string GetKind(this CommandInputSchema schema) =>
        schema switch
        {
            CommandParameterSchema => "Parameter",
            CommandOptionSchema => "Option",
            _ => throw new InvalidOperationException("Unknown input schema type.")
        };

    public static string GetFormattedIdentifier(this CommandInputSchema schema) =>
        schema switch
        {
            CommandParameterSchema parameter
                => parameter.IsSequence ? $"<{parameter.Name}>" : $"<{parameter.Name}...>",
            CommandOptionSchema option
                => option switch
                {
                    { Name: not null, ShortName: not null }
                        => $"-{option.ShortName}|--{option.Name}",
                    { Name: not null } => $"--{option.Name}",
                    { ShortName: not null } => $"-{option.ShortName}",
                    _
                        => throw new InvalidOperationException(
                            "Option must have a name or a short name."
                        )
                },
            _ => throw new ArgumentOutOfRangeException(nameof(schema))
        };
}
