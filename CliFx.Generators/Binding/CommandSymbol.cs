using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CliFx.Generators.Binding;

internal record CommandSymbol(
    TypeSymbol Type,
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

    internal static CommandSymbol? TryResolve(
        INamedTypeSymbol type,
        KnownSymbols knownSymbols,
        out IReadOnlyList<Diagnostic> diagnostics
    )
    {
        var diagnosticsList = new List<Diagnostic>();

        var classDeclaration = type
            .DeclaringSyntaxReferences.Select(r => r.GetSyntax())
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        // Must be partial to allow source generation to add members.
        if (
            classDeclaration is not null
            && !classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
        )
        {
            diagnosticsList.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.CommandMustBePartial,
                    classDeclaration.Identifier.GetLocation(),
                    type.Name
                )
            );
        }

        // Must implement ICommand
        if (
            !type.AllInterfaces.Any(i =>
                SymbolEqualityComparer.Default.Equals(i, knownSymbols.ICommand.Symbol)
            )
        )
        {
            diagnosticsList.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.CommandMustImplementICommand,
                    classDeclaration?.Identifier.GetLocation()
                        ?? type.Locations.FirstOrDefault()
                        ?? Location.None,
                    type.Name
                )
            );
        }

        // Must have the [Command] attribute
        var commandAttribute = type.GetAttributes()
            .FirstOrDefault(a =>
                SymbolEqualityComparer.Default.Equals(
                    a.AttributeClass,
                    knownSymbols.CommandAttribute.Symbol
                )
            );

        if (commandAttribute is null)
        {
            diagnostics = [];
            return null;
        }

        // Must be a concrete class
        if (type.IsAbstract)
        {
            diagnostics = [];
            return null;
        }

        var commandName =
            commandAttribute.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value
                as string
            ?? commandAttribute
                .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_String)
                .Select(a => a.Value as string)
                .FirstOrDefault();

        var commandDescription =
            commandAttribute.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
            as string;

        var parameters = new List<CommandParameterSymbol>();
        var options = new List<CommandOptionSymbol>();

        foreach (
            var property in type.GetProperties()
                .Where(p => !p.IsStatic)
                .Select(p => new PropertySymbol(p))
        )
        {
            var parameterAttribute = property
                .Symbol.GetAttributes()
                .FirstOrDefault(a =>
                    SymbolEqualityComparer.Default.Equals(
                        a.AttributeClass,
                        knownSymbols.CommandParameterAttribute.Symbol
                    )
                );

            if (parameterAttribute is not null)
            {
                var descriptor = CommandParameterSymbol.TryResolve(
                    property,
                    parameterAttribute,
                    out var parameterDiagnostics
                );

                diagnosticsList.AddRange(parameterDiagnostics);

                if (descriptor is not null)
                    parameters.Add(descriptor);

                continue;
            }

            var optionAttribute = property
                .Symbol.GetAttributes()
                .FirstOrDefault(a =>
                    SymbolEqualityComparer.Default.Equals(
                        a.AttributeClass,
                        knownSymbols.CommandOptionAttribute.Symbol
                    )
                );

            if (optionAttribute is not null)
            {
                var descriptor = CommandOptionSymbol.TryResolve(
                    property,
                    optionAttribute,
                    out var optionDiagnostics
                );

                diagnosticsList.AddRange(optionDiagnostics);

                if (descriptor is not null)
                    options.Add(descriptor);
            }
        }

        // Options must have unique names and short names
        for (var i = 0; i < options.Count; i++)
        {
            for (var j = i + 1; j < options.Count; j++)
            {
                var first = options[i];
                var second = options[j];

                if (
                    !string.IsNullOrWhiteSpace(first.Name)
                    && string.Equals(first.Name, second.Name, StringComparison.OrdinalIgnoreCase)
                )
                {
                    diagnosticsList.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.OptionsMustHaveUniqueNames,
                            second.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                            second.Property.Name,
                            type.Name,
                            "name",
                            second.Name,
                            first.Property.Name
                        )
                    );
                }

                if (first.ShortName is not null && first.ShortName == second.ShortName)
                {
                    diagnosticsList.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.OptionsMustHaveUniqueNames,
                            second.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                            second.Property.Name,
                            type.Name,
                            "short name",
                            second.ShortName.ToString(),
                            first.Property.Name
                        )
                    );
                }
            }
        }

        // Parameters must have unique order values
        for (var i = 0; i < parameters.Count; i++)
        {
            for (var j = i + 1; j < parameters.Count; j++)
            {
                var first = parameters[i];
                var second = parameters[j];

                if (first.Order == second.Order)
                {
                    diagnosticsList.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ParametersMustHaveUniqueOrder,
                            second.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                            second.Property.Name,
                            type.Name,
                            second.Order,
                            first.Property.Name
                        )
                    );
                }
            }
        }

        // Parameters must have unique names
        for (var i = 0; i < parameters.Count; i++)
        {
            for (var j = i + 1; j < parameters.Count; j++)
            {
                var first = parameters[i];
                var second = parameters[j];

                if (string.Equals(first.Name, second.Name, StringComparison.OrdinalIgnoreCase))
                {
                    diagnosticsList.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ParametersMustHaveUniqueNames,
                            second.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                            second.Property.Name,
                            type.Name,
                            second.Name,
                            first.Property.Name
                        )
                    );
                }
            }
        }

        diagnostics = diagnosticsList;

        return new CommandSymbol(
            new TypeSymbol(type),
            commandName,
            commandDescription,
            [.. parameters, .. options]
        );
    }
}
