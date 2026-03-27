using System;
using System.Collections.Generic;
using System.Linq;
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

    internal static CommandSymbol? TryResolve(
        INamedTypeSymbol type,
        out IReadOnlyList<Diagnostic> diagnostics
    )
    {
        var diagnosticsList = new List<Diagnostic>();
        diagnostics = diagnosticsList;

        var classDeclarations = type.GetDeclarations().ToArray();

        // Must have the [Command] attribute
        var commandAttribute = type.GetAttributes()
            .FirstOrDefault(a =>
                a.AttributeClass?.IsMatchedBy(KnownTypes.CommandAttribute) == true
            );

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
        if (!type.AllInterfaces.Any(i => i.IsMatchedBy(KnownTypes.ICommand)))
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
                    a.AttributeClass?.IsMatchedBy(KnownTypes.CommandParameterAttribute) == true
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
                    a.AttributeClass?.IsMatchedBy(KnownTypes.CommandOptionAttribute) == true
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

        // Element converter must not be sequence-based
        foreach (var input in parameters.Cast<CommandInputSymbol>().Concat(options))
        {
            if (input.IsElementConverter && input.IsConverterSequenceBased)
            {
                diagnosticsList.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.CommandInputElementConverterMustNotBeSequenceBased,
                        input.Property.Locations.FirstOrDefault(),
                        input.Property.Name
                    )
                );
            }
        }

        // Parameters must have unique order values
        foreach (var (i, first) in parameters.Index())
        {
            foreach (var second in parameters.Skip(i + 1))
            {
                if (first.Order == second.Order)
                {
                    diagnosticsList.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandParameterMustHaveUniqueOrder,
                            first.Property.Locations.FirstOrDefault(),
                            first.Property.Name,
                            second.Property.Name,
                            first.Order
                        )
                    );
                }
            }
        }

        // Parameters must have unique names
        foreach (var (i, first) in parameters.Index())
        {
            foreach (var second in parameters.Skip(i + 1))
            {
                if (string.Equals(first.Name, second.Name, StringComparison.OrdinalIgnoreCase))
                {
                    diagnosticsList.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandParameterMustHaveUniqueName,
                            first.Property.Locations.FirstOrDefault(),
                            first.Property.Name,
                            second.Property.Name,
                            first.Name
                        )
                    );
                }
            }
        }

        // Non-required parameters must have the highest order (and be alone)
        var parametersByOrder = parameters.OrderBy(p => p.Order).ToArray();
        foreach (var (i, parameter) in parametersByOrder.Index())
        {
            if (parameter.IsRequired)
                continue;

            if (i < parametersByOrder.Length - 1)
            {
                var nextParameter = parametersByOrder[i + 1];

                diagnosticsList.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.CommandParameterMustHaveHighestOrderIfNotRequired,
                        parameter.Property.Locations.FirstOrDefault(),
                        parameter.Property.Name,
                        nextParameter.Property.Name
                    )
                );
            }
        }

        // Sequence-based parameters must have the highest order (and be alone)
        foreach (var (i, parameter) in parametersByOrder.Index())
        {
            if (!parameter.IsSequenceBased)
                continue;

            if (i < parametersByOrder.Length - 1)
            {
                var nextParameter = parametersByOrder[i + 1];

                diagnosticsList.Add(
                    Diagnostic.Create(
                        DiagnosticDescriptors.CommandParameterMustHaveHighestOrderIfSequenceBased,
                        parameter.Property.Locations.FirstOrDefault(),
                        parameter.Property.Name,
                        nextParameter.Property.Name
                    )
                );
            }
        }

        // Options must have unique names and short names
        foreach (var (i, first) in options.Index())
        {
            foreach (var second in options.Skip(i + 1))
            {
                if (
                    !string.IsNullOrWhiteSpace(first.Name)
                    && string.Equals(first.Name, second.Name, StringComparison.OrdinalIgnoreCase)
                )
                {
                    diagnosticsList.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandOptionMustHaveUniqueName,
                            first.Property.Locations.FirstOrDefault(),
                            first.Property.Name,
                            second.Property.Name,
                            first.Name
                        )
                    );
                }

                if (first.ShortName is not null && first.ShortName == second.ShortName)
                {
                    diagnosticsList.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandOptionMustHaveUniqueShortName,
                            first.Property.Locations.FirstOrDefault(),
                            first.Property.Name,
                            second.Property.Name,
                            first.ShortName
                        )
                    );
                }
            }
        }

        return new CommandSymbol(
            type,
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
