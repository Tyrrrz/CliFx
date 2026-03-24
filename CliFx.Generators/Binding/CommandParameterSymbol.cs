using System.Collections.Generic;
using System.Linq;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal record CommandParameterSymbol(
    IPropertySymbol Property,
    int Order,
    string Name,
    bool IsRequired,
    string? Description,
    ResolvedTypeIdentifier? ConverterType,
    IReadOnlyList<ResolvedTypeIdentifier> ValidatorTypes
) : CommandInputSymbol(Property, IsRequired, Description, ConverterType, ValidatorTypes)
{
    internal static CommandParameterSymbol? TryResolve(
        IPropertySymbol property,
        AttributeData attribute,
        out IReadOnlyList<Diagnostic> diagnostics
    )
    {
        var diagnosticsList = new List<Diagnostic>();

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

        diagnostics = diagnosticsList;

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
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Converter").Value.Value is INamedTypeSymbol converterTypeSymbol
                ? ResolvedTypeIdentifier.From(converterTypeSymbol)
                : null,
            attribute
                .NamedArguments.Where(a => a.Key == "Validators")
                .SelectMany(a => a.Value.Values)
                .Select(v => v.Value as INamedTypeSymbol)
                .WhereNotNull()
                .Select(ResolvedTypeIdentifier.From)
                .ToArray()
        );
    }
}
