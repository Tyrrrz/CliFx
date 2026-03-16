using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.SemanticModel;

internal record CommandDescriptor(
    TypeDescriptor Type,
    string? Name,
    string? Description,
    IReadOnlyList<CommandParameterDescriptor> Parameters,
    IReadOnlyList<CommandOptionDescriptor> Options,
    IReadOnlyList<Diagnostic> Diagnostics
)
{
    public bool IsDefault => string.IsNullOrWhiteSpace(Name);
}
