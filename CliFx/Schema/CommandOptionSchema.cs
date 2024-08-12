using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes an option input of a command.
/// </summary>
public class CommandOptionSchema(
    PropertyBinding property,
    string? name,
    char? shortName,
    string? environmentVariable,
    bool isRequired,
    string? description,
    IBindingConverter converter,
    IReadOnlyList<IBindingValidator> validators
) : CommandInputSchema(property, description, converter, validators)
{
    internal override string Kind => "Option";

    internal override string FormattedIdentifier
    {
        get
        {
            var buffer = new StringBuilder();

            // Short name
            if (ShortName is not null)
            {
                buffer.Append('-').Append(ShortName);
            }

            // Separator
            if (!string.IsNullOrWhiteSpace(Name) && ShortName is not null)
            {
                buffer.Append('|');
            }

            // Name
            if (!string.IsNullOrWhiteSpace(Name))
            {
                buffer.Append("--").Append(Name);
            }

            return buffer.ToString();
        }
    }

    /// <summary>
    /// Option name.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Option short name.
    /// </summary>
    public char? ShortName { get; } = shortName;

    /// <summary>
    /// Environment variable that can be used as a fallback for this option.
    /// </summary>
    public string? EnvironmentVariable { get; } = environmentVariable;

    /// <summary>
    /// Whether the option is required.
    /// </summary>
    public bool IsRequired { get; } = isRequired;

    internal bool MatchesName(string? name) =>
        !string.IsNullOrWhiteSpace(Name)
        && string.Equals(Name, name, StringComparison.OrdinalIgnoreCase);

    internal bool MatchesShortName(char? shortName) =>
        ShortName is not null && ShortName == shortName;

    internal bool MatchesIdentifier(string identifier) =>
        MatchesName(identifier) || identifier.Length == 1 && MatchesShortName(identifier[0]);

    internal bool MatchesEnvironmentVariable(string environmentVariableName) =>
        !string.IsNullOrWhiteSpace(EnvironmentVariable)
        && string.Equals(EnvironmentVariable, environmentVariableName, StringComparison.Ordinal);
}

/// <inheritdoc cref="CommandOptionSchema" />
/// <remarks>
/// Generic version of the type is used to simplify initialization from source-generated code and
/// to enforce static references to all types used in the binding.
/// The non-generic version is used internally by the framework when operating in a dynamic context.
/// </remarks>
public class CommandOptionSchema<
    TCommand,
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicMethods
    )]
        TProperty
>(
    PropertyBinding<TCommand, TProperty> property,
    string? name,
    char? shortName,
    string? environmentVariable,
    bool isRequired,
    string? description,
    BindingConverter<TProperty> converter,
    IReadOnlyList<BindingValidator<TProperty>> validators
)
    : CommandOptionSchema(
        property,
        name,
        shortName,
        environmentVariable,
        isRequired,
        description,
        converter,
        validators
    )
    where TCommand : ICommand;
