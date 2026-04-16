using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Generators.Utils;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;
using PowerKit.Extensions;

namespace CliFx.Generators.Binding;

internal partial record CommandOptionSymbol(
    IPropertySymbol Property,
    string? Name,
    char? ShortName,
    string? EnvironmentVariable,
    bool IsRequired,
    string? Description,
    INamedTypeSymbol? ConverterType,
    IReadOnlyList<INamedTypeSymbol> ValidatorTypes
) : CommandInputSymbol(Property, IsRequired, Description, ConverterType, ValidatorTypes)
{
    internal string ToString(bool includeKind, bool includeValue)
    {
        var buffer = new StringBuilder();

        if (includeKind)
            buffer.Append("Option ");

        if (!string.IsNullOrWhiteSpace(Name) && ShortName is not null)
            buffer.Append($"-{ShortName}|--{Name}");
        else if (!string.IsNullOrWhiteSpace(Name))
            buffer.Append($"--{Name}");
        else if (ShortName is not null)
            buffer.Append($"-{ShortName}");

        if (includeValue)
        {
            buffer.Append(' ');

            if (!IsRequired)
                buffer.Append('<').Append("value").Append("?>");
            else if (IsSequenceBased)
                buffer.Append('<').Append("value").Append("...>");
            else
                buffer.Append('<').Append("value").Append('>');
        }

        return buffer.ToString();
    }

    public override string ToString() => ToString(true, true);
}

internal partial record CommandOptionSymbol
{
    internal static CommandOptionSymbol? TryResolve(
        IPropertySymbol property,
        DiagnosticReporter diagnostics
    )
    {
        var attribute = property.TryGetAttribute("CliFx.Binding.CommandOptionAttribute");
        if (attribute is null)
            return null;

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
            diagnostics.Report(
                DiagnosticDescriptors.CommandOptionMustHaveNameOrShortName,
                property.Locations.FirstOrDefault(),
                property.Name
            );
        }

        // Option name must be longer than one character, must start with a letter, and must not contain whitespace
        if (
            !string.IsNullOrWhiteSpace(name)
            && (name.Length < 2 || !char.IsLetter(name[0]) || name.Any(char.IsWhiteSpace))
        )
        {
            diagnostics.Report(
                DiagnosticDescriptors.CommandOptionMustHaveValidName,
                property.Locations.FirstOrDefault(),
                property.Name,
                name
            );
        }

        // Option short name must be a letter
        if (shortName is not null && !char.IsLetter(shortName.Value))
        {
            diagnostics.Report(
                DiagnosticDescriptors.CommandOptionMustHaveValidShortName,
                property.Locations.FirstOrDefault(),
                property.Name,
                shortName
            );
        }

        var environmentVariable =
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "EnvironmentVariable").Value.Value
            as string;

        var description =
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
            as string;

        return new CommandOptionSymbol(
            property,
            name,
            shortName,
            environmentVariable,
            property.IsRequired,
            description,
            TryResolveConverterType(attribute),
            ResolveValidatorTypes(attribute)
        );
    }

    internal static IReadOnlyList<CommandOptionSymbol> Resolve(
        IReadOnlyList<IPropertySymbol> properties,
        DiagnosticReporter diagnostics
    )
    {
        var options = properties.Select(p => TryResolve(p, diagnostics)).WhereNotNull().ToArray();

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
                    diagnostics.Report(
                        DiagnosticDescriptors.CommandOptionMustHaveUniqueName,
                        first.Property.Locations.FirstOrDefault(),
                        first.Property.Name,
                        second.Property.Name,
                        first.Name
                    );
                }

                if (first.ShortName is not null && first.ShortName == second.ShortName)
                {
                    diagnostics.Report(
                        DiagnosticDescriptors.CommandOptionMustHaveUniqueShortName,
                        first.Property.Locations.FirstOrDefault(),
                        first.Property.Name,
                        second.Property.Name,
                        first.ShortName
                    );
                }
            }
        }

        return options;
    }
}
