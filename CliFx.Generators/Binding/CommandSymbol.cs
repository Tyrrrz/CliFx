using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Generators.Utils;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal record CommandSymbol(
    INamedTypeSymbol Type,
    string? Name,
    string? Description,
    IReadOnlyList<CommandInputSymbol> Inputs
)
{
    public bool IsDefault => string.IsNullOrWhiteSpace(Name);

    public IReadOnlyList<CommandParameterSymbol> Parameters { get; } =
        Inputs.OfType<CommandParameterSymbol>().ToArray();

    public IReadOnlyList<CommandOptionSymbol> Options { get; } =
        Inputs.OfType<CommandOptionSymbol>().ToArray();

    internal static CommandSymbol? TryResolve(INamedTypeSymbol type, DiagnosticReporter diagnostics)
    {
        // Must have the [Command] attribute
        var attribute = type.TryGetAttribute("CliFx.Binding.CommandAttribute");
        if (attribute is null)
        {
            // Shouldn't happen when called by the generator since it filters types by attribute,
            // so not worth the effort to produce a diagnostic.
            return null;
        }

        // Must be partial by itself, along with all containing types
        foreach (var declaration in type.GetDeclarations().ToArray())
        {
            if (!declaration.IsFullyPartial())
            {
                diagnostics.Report(
                    DiagnosticDescriptors.CommandMustBePartial,
                    declaration.Identifier.GetLocation(),
                    type.Name
                );
            }
        }

        // Must implement ICommand
        if (!type.Implements("CliFx.ICommand"))
        {
            diagnostics.Report(
                DiagnosticDescriptors.CommandMustImplementICommand,
                type.Locations.FirstOrDefault(),
                type.Name
            );
        }

        var name =
            attribute?.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value as string
            ?? attribute
                ?.ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_String)
                .Select(a => a.Value as string)
                .FirstOrDefault();

        var description =
            attribute?.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
            as string;

        var properties = type.GetProperties().ToArray();
        var parameters = CommandParameterSymbol.Resolve(properties, diagnostics);
        var options = CommandOptionSymbol.Resolve(properties, diagnostics);

        return new CommandSymbol(type, name, description, [.. parameters, .. options]);
    }
}
