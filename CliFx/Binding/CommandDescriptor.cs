using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CliFx.Binding;

/// <summary>
/// Describes a binding between a class and a command.
/// </summary>
public class CommandDescriptor(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type type,
    string? name,
    string? description,
    IReadOnlyList<CommandInputDescriptor> inputs
)
{
    /// <summary>
    /// CLR type of the command.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type Type { get; } = type;

    /// <inheritdoc cref="CommandAttribute.Name" />
    public string? Name { get; } = name;

    internal bool IsDefault => string.IsNullOrWhiteSpace(Name);

    /// <inheritdoc cref="CommandAttribute.Description" />
    public string? Description { get; } = description;

    /// <summary>
    /// Inputs (parameters and options) of the command;
    /// </summary>
    public IReadOnlyList<CommandInputDescriptor> Inputs { get; } = inputs;

    internal IReadOnlyList<CommandParameterDescriptor> Parameters { get; } =
        inputs.OfType<CommandParameterDescriptor>().ToArray();

    internal IReadOnlyList<CommandOptionDescriptor> Options { get; } =
        inputs.OfType<CommandOptionDescriptor>().ToArray();

    internal bool MatchesName(string? name) =>
        !string.IsNullOrWhiteSpace(Name)
            ? string.Equals(name, Name, StringComparison.OrdinalIgnoreCase)
            : string.IsNullOrWhiteSpace(name);
}

/// <inheritdoc cref="CommandDescriptor" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public class CommandDescriptor<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        TCommand
>(string? name, string? description, IReadOnlyList<CommandInputDescriptor> inputs)
    : CommandDescriptor(typeof(TCommand), name, description, inputs)
    where TCommand : ICommand;
