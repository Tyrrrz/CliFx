using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes an option input of a command.
/// </summary>
public class CommandOptionSchema(
    PropertyBinding property,
    bool isSequence,
    string? name,
    char? shortName,
    string? environmentVariable,
    bool isRequired,
    string? description,
    IBindingConverter? converter,
    IReadOnlyList<IBindingValidator> validators,
    ISequenceBindingConverter? sequenceConverter = null
) : CommandInputSchema(property, isSequence, description, converter, validators, sequenceConverter)
{
    /// <summary>
    /// Option name (the --name part).
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Option short name (the -n part).
    /// </summary>
    public char? ShortName { get; } = shortName;

    /// <summary>
    /// Environment variable used as a fallback for this option.
    /// </summary>
    public string? EnvironmentVariable { get; } = environmentVariable;

    /// <summary>
    /// Whether this option must be provided.
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
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public class CommandOptionSchema<
    TCommand,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TProperty
>(
    PropertyBinding<TCommand, TProperty> property,
    bool isSequence,
    string? name,
    char? shortName,
    string? environmentVariable,
    bool isRequired,
    string? description,
    BindingConverter<TProperty>? converter,
    IReadOnlyList<IBindingValidator> validators
)
    : CommandOptionSchema(
        property,
        isSequence,
        name,
        shortName,
        environmentVariable,
        isRequired,
        description,
        converter,
        validators
    )
    where TCommand : ICommand;
