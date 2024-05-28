using System;
using System.Collections.Generic;
using System.Text;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes a command's option.
/// </summary>
public class OptionSchema(
    PropertyDescriptor property,
    string? name,
    char? shortName,
    string? environmentVariable,
    bool isRequired,
    string? description,
    IBindingConverter? converter,
    IReadOnlyList<IBindingValidator> validators
) : IInputSchema
{
    /// <inheritdoc />
    public PropertyDescriptor Property { get; } = property;

    /// <inheritdoc />
    public bool IsScalar { get; }

    /// <inheritdoc />
    public IReadOnlyList<object?>? ValidValues { get; }

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

    /// <summary>
    /// Option description.
    /// </summary>
    public string? Description { get; } = description;

    /// <inheritdoc />
    public IBindingConverter? Converter { get; } = converter;

    /// <inheritdoc />
    public IReadOnlyList<IBindingValidator> Validators { get; } = validators;

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

    internal string GetFormattedIdentifier()
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
