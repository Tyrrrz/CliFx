using System.Collections.Generic;
using System.Linq;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal record CommandParameterSymbol(
    PropertySymbol Property,
    int Order,
    string Name,
    bool IsRequired,
    string? Description,
    TypeSymbol? ConverterType,
    IReadOnlyList<TypeSymbol> ValidatorTypes
) : CommandInputSymbol(Property, IsRequired, Description, ConverterType, ValidatorTypes)
{
    internal static CommandParameterSymbol? TryResolve(
        PropertySymbol property,
        AttributeData attribute,
        out IReadOnlyList<Diagnostic> diagnostics
    )
    {
        var diagnosticsList = new List<Diagnostic>();

        var order = attribute
            .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_Int32)
            .Select(a => (int)(a.Value ?? 0))
            .FirstOrDefault();

        var explicitName =
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value as string;

        // Explicit parameter name must not be empty
        if (explicitName is not null && string.IsNullOrWhiteSpace(explicitName))
        {
            diagnosticsList.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.ParameterMustHaveName,
                    property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                    property.Name
                )
            );
        }

        var name = explicitName ?? property.Name.ToLowerInvariant();

        var converterType = attribute
            .NamedArguments.FirstOrDefault(a => a.Key == "Converter")
            .Value.Value
            is ITypeSymbol converterTypeSymbol
            ? new TypeSymbol(converterTypeSymbol)
            : null;

        diagnostics = diagnosticsList;

        return new CommandParameterSymbol(
            property,
            order,
            name,
            property.IsRequired,
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
                as string,
            converterType,
            attribute
                .NamedArguments.Where(a => a.Key == "Validators")
                .SelectMany(a => a.Value.Values)
                .Select(v => v.Value as ITypeSymbol)
                .WhereNotNull()
                .ToArray()
                .Select(s => new TypeSymbol(s))
                .ToArray()
        );
    }
}
