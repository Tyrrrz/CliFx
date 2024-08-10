using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CliFx.Schema;

/// <summary>
/// Describes an individual command, along with its parameter and option inputs.
/// </summary>
public class CommandSchema(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type,
    string? name,
    string? description,
    IReadOnlyList<InputSchema> inputs
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
    /// Whether the command is the application's default command.
    /// </summary>
    public bool IsDefault { get; } = string.IsNullOrWhiteSpace(name);

    /// <summary>
    /// Command description.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Inputs (parameters and options) of the command.
    /// </summary>
    public IReadOnlyList<InputSchema> Inputs { get; } = inputs;

    /// <summary>
    /// Parameter inputs of the command.
    /// </summary>
    public IReadOnlyList<ParameterSchema> Parameters { get; } =
        inputs.OfType<ParameterSchema>().ToArray();

    /// <summary>
    /// Option inputs of the command.
    /// </summary>
    public IReadOnlyList<OptionSchema> Options { get; } = inputs.OfType<OptionSchema>().ToArray();

    internal bool MatchesName(string? name) =>
        !string.IsNullOrWhiteSpace(Name)
            ? string.Equals(name, Name, StringComparison.OrdinalIgnoreCase)
            : string.IsNullOrWhiteSpace(name);

    internal IReadOnlyDictionary<InputSchema, object?> GetValues(ICommand instance)
    {
        var result = new Dictionary<InputSchema, object?>();

        foreach (var parameterSchema in Parameters)
        {
            var value = parameterSchema.Property.Get(instance);
            result[parameterSchema] = value;
        }

        foreach (var optionSchema in Options)
        {
            var value = optionSchema.Property.Get(instance);
            result[optionSchema] = value;
        }

        return result;
    }
}

// Generic version of the type is used to simplify initialization from the source-generated code
// and to enforce static references to all the types used in the binding.
// The non-generic version is used internally by the framework when operating in a dynamic context.
/// <inheritdoc cref="CommandSchema" />
public class CommandSchema<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand
>(string? name, string? description, IReadOnlyList<InputSchema> inputs)
    : CommandSchema(typeof(TCommand), name, description, inputs)
    where TCommand : ICommand;
