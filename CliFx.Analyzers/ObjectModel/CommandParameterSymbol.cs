using System.Collections.Generic;
using System.Linq;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers.ObjectModel
{
    internal partial class CommandParameterSymbol
    {
        public int Order { get; }

        public string? Name { get; }

        public ITypeSymbol? ConverterType { get; }

        public IReadOnlyList<ITypeSymbol> ValidatorTypes { get; }

        public CommandParameterSymbol(
            int order,
            string? name,
            ITypeSymbol? converterType,
            IReadOnlyList<ITypeSymbol> validatorTypes)
        {
            Order = order;
            Name = name;
            ConverterType = converterType;
            ValidatorTypes = validatorTypes;
        }
    }

    internal partial class CommandParameterSymbol
    {
        private static AttributeData? TryGetParameterAttribute(IPropertySymbol property) =>
            property
                .GetAttributes()
                .FirstOrDefault(a => a.AttributeClass.DisplayNameMatches(SymbolNames.CliFxCommandParameterAttribute));

        private static CommandParameterSymbol FromAttribute(AttributeData attribute)
        {
            var order = (int) attribute
                .ConstructorArguments
                .Select(a => a.Value)
                .First()!;

            var name = attribute
                .NamedArguments
                .Where(a => a.Key == "Name")
                .Select(a => a.Value.Value)
                .FirstOrDefault() as string;

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

            return new CommandParameterSymbol(order, name, converter, validators);
        }

        public static CommandParameterSymbol? TryResolve(IPropertySymbol property)
        {
            var attribute = TryGetParameterAttribute(property);

            if (attribute is null)
                return null;

            return FromAttribute(attribute);
        }

        public static bool IsParameterProperty(IPropertySymbol property) =>
            TryGetParameterAttribute(property) is not null;
    }
}