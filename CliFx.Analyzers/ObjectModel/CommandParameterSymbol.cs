using System.Collections.Generic;
using System.Linq;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers.ObjectModel;

internal partial class CommandParameterSymbol(
    IPropertySymbol property,
    int order,
    string? name,
    bool? isRequired,
    ITypeSymbol? converterType,
    IReadOnlyList<ITypeSymbol> validatorTypes
) : ICommandMemberSymbol
{
    public IPropertySymbol Property { get; } = property;

    public int Order { get; } = order;

    public string? Name { get; } = name;

    public bool? IsRequired { get; } = isRequired;

    public ITypeSymbol? ConverterType { get; } = converterType;

    public IReadOnlyList<ITypeSymbol> ValidatorTypes { get; } = validatorTypes;
}

internal partial class CommandParameterSymbol
{
    private static AttributeData? TryGetParameterAttribute(IPropertySymbol property) =>
        property
            .GetAttributes()
            .FirstOrDefault(a =>
                a.AttributeClass?.DisplayNameMatches(SymbolNames.CliFxCommandParameterAttribute)
                == true
            );

    public static CommandParameterSymbol? TryResolve(IPropertySymbol property)
    {
        var attribute = TryGetParameterAttribute(property);
        if (attribute is null)
            return null;

        var order = (int)attribute.ConstructorArguments.Select(a => a.Value).First()!;

        var name =
            attribute
                .NamedArguments.Where(a => a.Key == "Name")
                .Select(a => a.Value.Value)
                .FirstOrDefault() as string;

        var isRequired =
            attribute
                .NamedArguments.Where(a => a.Key == "IsRequired")
                .Select(a => a.Value.Value)
                .FirstOrDefault() as bool?;

        var converter = attribute
            .NamedArguments.Where(a => a.Key == "Converter")
            .Select(a => a.Value.Value)
            .Cast<ITypeSymbol?>()
            .FirstOrDefault();

        var validators = attribute
            .NamedArguments.Where(a => a.Key == "Validators")
            .SelectMany(a => a.Value.Values)
            .Select(c => c.Value)
            .Cast<ITypeSymbol>()
            .ToArray();

        return new CommandParameterSymbol(property, order, name, isRequired, converter, validators);
    }

    public static bool IsParameterProperty(IPropertySymbol property) =>
        TryGetParameterAttribute(property) is not null;
}
