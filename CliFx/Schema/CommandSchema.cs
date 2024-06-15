using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CliFx.Schema;

/// <summary>
/// Describes an individual command, with its parameter and option bindings.
/// </summary>
public class CommandSchema(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type,
    string? name,
    string? description,
    IReadOnlyList<ParameterSchema> parameters,
    IReadOnlyList<OptionSchema> options
)
{
    /// <summary>
    /// Underlying CLR type of the command.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type Type { get; } = type;

    /// <summary>
    /// Command name.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Whether this command is the application's default command.
    /// </summary>
    public bool IsDefault { get; } = string.IsNullOrWhiteSpace(name);

    /// <summary>
    /// Command description.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Command parameters.
    /// </summary>
    public IReadOnlyList<ParameterSchema> Parameters { get; } = parameters;

    /// <summary>
    /// Command options.
    /// </summary>
    public IReadOnlyList<OptionSchema> Options { get; } = options;

    internal bool MatchesName(string? name) =>
        !string.IsNullOrWhiteSpace(Name)
            ? string.Equals(name, Name, StringComparison.OrdinalIgnoreCase)
            : string.IsNullOrWhiteSpace(name);

    internal IReadOnlyDictionary<InputSchema, object?> GetValues(ICommand instance)
    {
        var result = new Dictionary<InputSchema, object?>();

        foreach (var parameterSchema in Parameters)
        {
            var value = parameterSchema.Property.GetValue(instance);
            result[parameterSchema] = value;
        }

        foreach (var optionSchema in Options)
        {
            var value = optionSchema.Property.GetValue(instance);
            result[optionSchema] = value;
        }

        return result;
    }
}
