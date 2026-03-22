using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal record CommandOptionSymbol(
    PropertySymbol Property,
    string? Name,
    char? ShortName,
    string? EnvironmentVariable,
    bool IsRequired,
    string? Description,
    TypeSymbol? ConverterType,
    IReadOnlyList<TypeSymbol> ValidatorTypes
) : CommandInputSymbol(Property, IsRequired, Description, ConverterType, ValidatorTypes)
{
    internal static CommandOptionSymbol? TryResolve(
        PropertySymbol property,
        AttributeData attribute,
        out IReadOnlyList<Diagnostic> diagnostics
    )
    {
        var diagnosticsList = new List<Diagnostic>();

        var name =
            attribute
                .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_String)
                .Select(a => a.Value as string)
                .FirstOrDefault()
            ?? attribute.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value as string;

        var shortName =
            attribute
                .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_Char)
                .Select(a => a.Value as char?)
                .FirstOrDefault()
            ?? attribute.NamedArguments.FirstOrDefault(a => a.Key == "ShortName").Value.Value
                as char?;

        // Option must have either a name or short name, or both
        if (string.IsNullOrWhiteSpace(name) && shortName is null)
        {
            diagnosticsList.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.OptionMustHaveNameOrShortName,
                    property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                    property.Name
                )
            );
        }

        // Option name must be longer than one character, must start with a letter, and must not contain whitespace
        if (
            !string.IsNullOrWhiteSpace(name)
            && (name.Length < 2 || !char.IsLetter(name[0]) || name.Any(char.IsWhiteSpace))
        )
        {
            diagnosticsList.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.OptionNameInvalid,
                    property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                    name,
                    property.Name
                )
            );
        }

        var converterType = attribute
            .NamedArguments.FirstOrDefault(a => a.Key == "Converter")
            .Value.Value
            is ITypeSymbol converterTypeSymbol
            ? new TypeSymbol(converterTypeSymbol)
            : null;

        diagnostics = diagnosticsList;

        return new CommandOptionSymbol(
            property,
            name,
            shortName,
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "EnvironmentVariable").Value.Value
                as string,
            property.IsRequired,
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
                as string,
            converterType,
            attribute
                .NamedArguments.Where(a => a.Key == "Validators")
                .SelectMany(a => a.Value.Values)
                .Select(v => v.Value)
                .OfType<ITypeSymbol>()
                .Select(s => new TypeSymbol(s))
                .ToArray()
        );
    }
}
