using System.Collections.Generic;
using System.Linq;
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
        AttributeData attribute,
        out IReadOnlyList<Diagnostic> diagnostics
    )
    {
        var diagnosticsList = new List<Diagnostic>();
        diagnostics = diagnosticsList;

        var name =
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value as string;

        // Explicit parameter name must not be empty
        if (name is not null && string.IsNullOrWhiteSpace(name))
        {
            diagnosticsList.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.CommandParameterMustHaveName,
                    property.Locations.FirstOrDefault(),
                    property.Name
                )
            );
        }

        return new CommandParameterSymbol(
            property,
            attribute
                .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_Int32)
                .Select(a => (int)(a.Value ?? 0))
                .FirstOrDefault(),
            name ?? property.Name.ToLowerInvariant(),
            property.IsRequired,
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
                as string,
            TryResolveConverterType(attribute),
            ResolveValidatorTypes(attribute)
        );
    }
}
