using System.Collections.Generic;
using System.Linq;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers.ObjectModel;

internal partial class CommandOptionSymbol : ICommandMemberSymbol
{
    public IPropertySymbol Property { get; }

    public string? Name { get; }

    public char? ShortName { get; }

    public ITypeSymbol? ConverterType { get; }

    public IReadOnlyList<ITypeSymbol> ValidatorTypes { get; }

    public CommandOptionSymbol(
        IPropertySymbol property,
        string? name,
        char? shortName,
        ITypeSymbol? converterType,
        IReadOnlyList<ITypeSymbol> validatorTypes)
    {
        Property = property;
        Name = name;
        ShortName = shortName;
        ConverterType = converterType;
        ValidatorTypes = validatorTypes;
    }
}

internal partial class CommandOptionSymbol
{
    private static AttributeData? TryGetOptionAttribute(IPropertySymbol property) => property
        .GetAttributes()
        .FirstOrDefault(a => a.AttributeClass?.DisplayNameMatches(SymbolNames.CliFxCommandOptionAttribute) == true);

    public static CommandOptionSymbol? TryResolve(IPropertySymbol property)
    {
        var attribute = TryGetOptionAttribute(property);
        if (attribute is null)
            return null;

        var name = attribute
            .ConstructorArguments
            .Where(a => a.Type?.SpecialType == SpecialType.System_String)
            .Select(a => a.Value)
            .FirstOrDefault() as string;

        var shortName = attribute
            .ConstructorArguments
            .Where(a => a.Type?.SpecialType == SpecialType.System_Char)
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

        return new CommandOptionSymbol(property, name, shortName, converter, validators);
    }

    public static bool IsOptionProperty(IPropertySymbol property) =>
        TryGetOptionAttribute(property) is not null;
}