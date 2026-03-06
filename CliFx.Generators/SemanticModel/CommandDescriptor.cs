using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal record CommandDescriptor(
    TypeDescriptor Type,
    string? Name,
    string? Description,
    IReadOnlyList<CommandParameterDescriptor> Parameters,
    IReadOnlyList<CommandOptionDescriptor> Options,
    IReadOnlyList<Diagnostic> Diagnostics
);
