using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Generators.Utils;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal record CommandParameterSymbol(
    IPropertySymbol Property,
    int Order,
    string Name,
    bool IsRequired,
    string? Description,
    INamedTypeSymbol? ConverterType,
    IReadOnlyList<INamedTypeSymbol> ValidatorTypes
) : CommandInputSymbol(Property, IsRequired, Description, ConverterType, ValidatorTypes)
{
    internal static CommandParameterSymbol? TryResolve(
        IPropertySymbol property,
        DiagnosticReporter diagnostics
    )
    {
        var attribute = property
            .GetAttributes()
            .FirstOrDefault(a =>
                a.AttributeClass?.IsMatchedBy("CliFx.Binding.CommandParameterAttribute") == true
            );

        if (attribute is null)
        {
            return null;
        }

        var order = attribute
            .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_Int32)
            .Select(a => (int)(a.Value ?? 0))
            .FirstOrDefault();

        var name =
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value as string;

        // Explicit name must not be empty
        if (name is not null && string.IsNullOrWhiteSpace(name))
        {
            diagnostics.Report(
                DiagnosticDescriptors.CommandParameterMustHaveName,
                property.Locations.FirstOrDefault(),
                property.Name
            );
        }

        var description =
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
            as string;

        return new CommandParameterSymbol(
            property,
            order,
            name ?? property.Name.ToLowerInvariant(),
            property.IsRequired,
            description,
            TryResolveConverterType(attribute),
            ResolveValidatorTypes(attribute)
        );
    }

    internal static IReadOnlyList<CommandParameterSymbol> Resolve(
        IReadOnlyList<IPropertySymbol> properties,
        DiagnosticReporter diagnostics
    )
    {
        var parameters = properties
            .Select(p => TryResolve(p, diagnostics))
            .WhereNotNull()
            .ToArray();

        // Parameters must have unique order values
        foreach (var (i, first) in parameters.Index())
        {
            foreach (var second in parameters.Skip(i + 1))
            {
                if (first.Order == second.Order)
                {
                    diagnostics.Report(
                        DiagnosticDescriptors.CommandParameterMustHaveUniqueOrder,
                        first.Property.Locations.FirstOrDefault(),
                        first.Property.Name,
                        second.Property.Name,
                        first.Order
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
                    diagnostics.Report(
                        DiagnosticDescriptors.CommandParameterMustHaveUniqueName,
                        first.Property.Locations.FirstOrDefault(),
                        first.Property.Name,
                        second.Property.Name,
                        first.Name
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

                diagnostics.Report(
                    DiagnosticDescriptors.CommandParameterMustHaveHighestOrderIfNotRequired,
                    parameter.Property.Locations.FirstOrDefault(),
                    parameter.Property.Name,
                    nextParameter.Property.Name
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

                diagnostics.Report(
                    DiagnosticDescriptors.CommandParameterMustHaveHighestOrderIfSequenceBased,
                    parameter.Property.Locations.FirstOrDefault(),
                    parameter.Property.Name,
                    nextParameter.Property.Name
                );
            }
        }

        return parameters;
    }
}
