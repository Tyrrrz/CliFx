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
        var classDeclarations = type.GetDeclarations().ToArray();

        // Must have the [Command] attribute
        var commandAttribute = type.GetAttributes()
            .FirstOrDefault(a =>
                a.AttributeClass?.IsMatchedBy("CliFx.Binding.CommandAttribute") == true
            );

        if (commandAttribute is null)
        {
            // Shouldn't happen when called by the generator since it filters types by attribute,
            // so not worth the effort to produce a diagnostic.
            return null;
        }

        // Must be partial by itself, along with all containing types
        foreach (var declaration in classDeclarations)
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
                classDeclarations.FirstOrDefault()?.Identifier.GetLocation()
                    ?? type.Locations.FirstOrDefault(),
                type.Name
            );
        }

        var name =
            commandAttribute?.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value
                as string
            ?? commandAttribute
                ?.ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_String)
                .Select(a => a.Value as string)
                .FirstOrDefault();

        var description =
            commandAttribute?.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
            as string;

        var properties = type.GetProperties().ToArray();
        var parameters = CommandParameterSymbol.Resolve(properties, diagnostics);
        var options = CommandOptionSymbol.Resolve(properties, diagnostics);

        return new CommandSymbol(type, name, description, [.. parameters, .. options]);
    }
}
