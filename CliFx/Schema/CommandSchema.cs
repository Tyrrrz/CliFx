using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CliFx.Extensibility;

namespace CliFx.Schema;

/// <summary>
/// Describes the schema of a command.
/// </summary>
public partial class CommandSchema(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type type,
    string? name,
    string? description,
    IReadOnlyList<CommandParameterSchema> parameters,
    IReadOnlyList<CommandOptionSchema> options
)
{
    /// <summary>
    /// CLR type of the command.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type Type { get; } = type;

    /// <summary>
    /// Command name. Null or empty for the default command.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Command description shown in help text.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Parameters of the command.
    /// </summary>
    public IReadOnlyList<CommandParameterSchema> Parameters { get; } = parameters;

    /// <summary>
    /// Options of the command, including implicit ones.
    /// </summary>
    public IReadOnlyList<CommandOptionSchema> Options { get; } = options;

    /// <summary>
    /// Whether this is the default command (no name).
    /// </summary>
    public bool IsDefault => string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// Whether the implicit --help option is available.
    /// </summary>
    public bool IsImplicitHelpOptionAvailable =>
        Options.Contains(CommandOptionSchema.ImplicitHelpOption);

    /// <summary>
    /// Whether the implicit --version option is available.
    /// </summary>
    public bool IsImplicitVersionOptionAvailable =>
        Options.Contains(CommandOptionSchema.ImplicitVersionOption);

    /// <summary>
    /// Whether this command matches the given name.
    /// </summary>
    public bool MatchesName(string? name) =>
        !string.IsNullOrWhiteSpace(Name)
            ? string.Equals(name, Name, StringComparison.OrdinalIgnoreCase)
            : string.IsNullOrWhiteSpace(name);

    internal IReadOnlyDictionary<CommandInputSchema, object?> GetValues(ICommand instance)
    {
        var result = new Dictionary<CommandInputSchema, object?>();

        foreach (var parameterSchema in Parameters)
        {
            var value = parameterSchema.Property.GetValue(instance);
            result[parameterSchema] = value;
        }

        foreach (var optionSchema in Options)
        {
            if (optionSchema.Property is NullPropertyBinding)
                continue;
            var value = optionSchema.Property.GetValue(instance);
            result[optionSchema] = value;
        }

        return result;
    }
}

/// <inheritdoc cref="CommandSchema" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public class CommandSchema<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        TCommand
>(string? name, string? description, IReadOnlyList<CommandInputSchema> inputs)
    : CommandSchema(
        typeof(TCommand),
        name,
        description,
        inputs.OfType<CommandParameterSchema>().ToArray(),
        AddImplicitOptions(name, inputs.OfType<CommandOptionSchema>().ToList())
    )
    where TCommand : ICommand
{
    private static IReadOnlyList<CommandOptionSchema> AddImplicitOptions(
        string? commandName,
        List<CommandOptionSchema> options
    )
    {
        if (!options.Any(o => o.MatchesShortName('h') || o.MatchesName("help")))
            options.Add(CommandOptionSchema.ImplicitHelpOption);

        if (string.IsNullOrWhiteSpace(commandName) && !options.Any(o => o.MatchesName("version")))
            options.Add(CommandOptionSchema.ImplicitVersionOption);

        return options;
    }
}

/// <summary>
/// Null property binding used for implicit options (help, version) that have no backing property.
/// </summary>
public sealed class NullPropertyBinding()
    : PropertyBinding(typeof(object), _ => null, (_, _) => { });
