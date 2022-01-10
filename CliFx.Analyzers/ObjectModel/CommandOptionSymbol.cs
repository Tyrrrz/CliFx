using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Linq;
using CliFx.Analyzers.Utils.Extensions;

namespace CliFx.Analyzers.ObjectModel;

internal partial class CommandOptionSymbol
{
    public string? Name { get; }

    public char? ShortName { get; }

    public ITypeSymbol? ConverterType { get; }

    public IReadOnlyList<ITypeSymbol> ValidatorTypes { get; }

    public CommandOptionSymbol(
        string? name,
        char? shortName,
        ITypeSymbol? converterType,
        IReadOnlyList<ITypeSymbol> validatorTypes)
    {
        Name = name;
        ShortName = shortName;
        ConverterType = converterType;
        ValidatorTypes = validatorTypes;
    }
}

internal partial class CommandOptionSymbol
{
    private static AttributeData? TryGetOptionAttribute(IPropertySymbol property) =>
        property
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass.DisplayNameMatches(SymbolNames.CliFxCommandOptionAttribute));

    private static CommandOptionSymbol FromAttribute(AttributeData attribute)
    {
        var name = attribute
            .ConstructorArguments
            .Where(a => a.Type.DisplayNameMatches("string") || a.Type.DisplayNameMatches("System.String"))
            .Select(a => a.Value)
            .FirstOrDefault() as string;

        var shortName = attribute
            .ConstructorArguments
            .Where(a => a.Type.DisplayNameMatches("char") || a.Type.DisplayNameMatches("System.Char"))
            .Select(a => a.Value)
            .FirstOrDefault() as char?;

        var converter = attribute
            .NamedArguments
            .Where(a => a.Key == "Converter")
            .Select(a => a.Value.Value)
            .Cast<ITypeSymbol?>()
            .FirstOrDefault();

        var validators = attribute
            .NamedArguments
            .Where(a => a.Key == "Validators")
            .SelectMany(a => a.Value.Values)
            .Select(c => c.Value)
            .Cast<ITypeSymbol>()
            .ToArray();

        return new CommandOptionSymbol(name, shortName, converter, validators);
    }

    public static CommandOptionSymbol? TryResolve(IPropertySymbol property)
    {
        var attribute = TryGetOptionAttribute(property);

        return attribute is not null
            ? FromAttribute(attribute)
            : null;
    }

    public static bool IsOptionProperty(IPropertySymbol property) =>
        TryGetOptionAttribute(property) is not null;
}