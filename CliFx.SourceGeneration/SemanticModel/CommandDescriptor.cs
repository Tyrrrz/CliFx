using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal class CommandDescriptor(
    INamedTypeSymbol type,
    string? name,
    string? description,
    IReadOnlyList<CommandParameterDescriptor> parameters,
    IReadOnlyList<CommandOptionDescriptor> options
)
{
    public INamedTypeSymbol Type { get; } = type;
    public string? Name { get; } = name;
    public string? Description { get; } = description;
    public IReadOnlyList<CommandParameterDescriptor> Parameters { get; } = parameters;
    public IReadOnlyList<CommandOptionDescriptor> Options { get; } = options;
}
