using System;
using System.Collections.Generic;
using CliFx.Activation;

namespace CliFx.Binding;

/// <summary>
/// Describes an option input of a command.
/// </summary>
public class CommandOptionDescriptor(
    PropertyDescriptor property,
    string? name,
    char? shortName,
    string? environmentVariable,
    bool isRequired,
    string? description,
    IInputConverter converter,
    IReadOnlyList<IInputValidator> validators
) : CommandInputDescriptor(property, isRequired, description, converter, validators)
{
    /// <inheritdoc cref="CommandOptionAttribute.Name" />
    public string? Name { get; } = name;

    /// <inheritdoc cref="CommandOptionAttribute.ShortName" />
    public char? ShortName { get; } = shortName;

    /// <inheritdoc cref="CommandOptionAttribute.EnvironmentVariable" />
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

/// <inheritdoc cref="CommandOptionDescriptor" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public class CommandOptionDescriptor<TCommand, TProperty>(
    PropertyDescriptor<TCommand, TProperty> property,
    string? name,
    char? shortName,
    string? environmentVariable,
    bool isRequired,
    string? description,
    InputConverter<TProperty> converter,
    IReadOnlyList<InputValidator<TProperty>> validators
)
    : CommandOptionDescriptor(
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
