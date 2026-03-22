using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal record CommandSymbol(
    TypeSymbol Type,
    string? Name,
    string? Description,
    IReadOnlyList<CommandInputSymbol> Inputs,
    IReadOnlyList<Diagnostic> Diagnostics
)
{
    public IReadOnlyList<CommandParameterSymbol> Parameters { get; } =
        Inputs.OfType<CommandParameterSymbol>().ToArray();

    public IReadOnlyList<CommandOptionSymbol> Options { get; } =
        Inputs.OfType<CommandOptionSymbol>().ToArray();

    public bool IsDefault => string.IsNullOrWhiteSpace(Name);
}
