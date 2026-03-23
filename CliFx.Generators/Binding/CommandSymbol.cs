using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal record CommandSymbol(
    ResolvedTypeIdentifier Type,
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
        out IReadOnlyList<Diagnostic> diagnostics
    )
    {
        var diagnosticsList = new List<Diagnostic>();
        var classDeclarations = type.GetDeclarations().ToArray();

        // Must have the [Command] attribute
        var commandAttribute = type.GetAttributes()
            .FirstOrDefault(a => KnownSymbols.CommandAttribute.IsMatchedBy(a.AttributeClass));

        if (commandAttribute is null)
        {
            // Shouldn't happen when called by the generator since it filters types by attribute,
            // so not worth the effort to produce a diagnostic.
            diagnostics = diagnosticsList;
            return null;
        }

        // Must be partial by itself, along with all containing types
        foreach (var declaration in classDeclarations)
        {
            if (!declaration.IsFullyPartial())
            {
                diagnosticsList.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.CommandMustBePartial,
                        declaration.Identifier.GetLocation(),
                        type.Name
                    )
                );
            }
        }

        // Must implement ICommand
        if (!type.AllInterfaces.Any(KnownSymbols.ICommand.IsMatchedBy))
        {
            diagnosticsList.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.CommandMustImplementICommand,
                    classDeclarations.FirstOrDefault()?.Identifier.GetLocation()
                        ?? type.Locations.FirstOrDefault(),
                    type.Name
                )
            );
        }

        var parameters = new List<CommandParameterSymbol>();
        var options = new List<CommandOptionSymbol>();

        foreach (var property in type.GetProperties().Where(p => !p.IsStatic))
        {
            var parameterAttribute = property
                .GetAttributes()
                .FirstOrDefault(a =>
                    KnownSymbols.CommandParameterAttribute.IsMatchedBy(a.AttributeClass)
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
            }

            var optionAttribute = property
                .GetAttributes()
                .FirstOrDefault(a =>
                    KnownSymbols.CommandOptionAttribute.IsMatchedBy(a.AttributeClass)
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
                            second.Property.Locations.FirstOrDefault() ?? Location.None,
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
                            second.Property.Locations.FirstOrDefault() ?? Location.None,
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
                            second.Property.Locations.FirstOrDefault() ?? Location.None,
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
                            second.Property.Locations.FirstOrDefault() ?? Location.None,
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
            ResolvedTypeIdentifier.From(type),
            commandAttribute?.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value
                as string
                ?? commandAttribute
                    ?.ConstructorArguments.Where(a =>
                        a.Type?.SpecialType == SpecialType.System_String
                    )
                    .Select(a => a.Value as string)
                    .FirstOrDefault(),
            commandAttribute?.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
                as string,
            [.. parameters, .. options]
        );
    }
}
