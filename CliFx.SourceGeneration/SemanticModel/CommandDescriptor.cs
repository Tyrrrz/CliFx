using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal record CommandDescriptor(
    INamedTypeSymbol Type,
    string? Name,
    string? Description,
    IReadOnlyList<CommandParameterDescriptor> Parameters,
    IReadOnlyList<CommandOptionDescriptor> Options,
    bool HasExistingSchemaProperty
);
