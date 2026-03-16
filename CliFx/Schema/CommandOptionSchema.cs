using System;
using System.Collections.Generic;
using CliFx.Infrastructure.Binding;

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
) : CommandInputSchema(property, isRequired, description, converter, validators)
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
public class CommandOptionSchema<TCommand, TProperty>(
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
