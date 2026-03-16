using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CliFx.Generators.Utils.Extensions;

internal static class RoslynExtensions
{
    public static IncrementalValuesProvider<T> WhereNotNull<T>(
        this IncrementalValuesProvider<T?> source
    )
        where T : class => source.Where(d => d is not null).Select((d, _) => d!);

    extension(ISymbol symbol)
    {
        public bool DisplayNameMatches(string name) =>
            string.Equals(
                symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                name,
                StringComparison.Ordinal
            );
    }

    extension(ITypeSymbol type)
    {
        public IEnumerable<ITypeSymbol> GetContainingTypes()
        {
            var containingType = type.ContainingType;
            while (containingType is not null)
            {
                yield return containingType;
                containingType = containingType.ContainingType;
            }
        }

        public IEnumerable<ITypeSymbol> GetBaseTypes()
        {
            var baseType = type.BaseType;
            while (baseType is not null)
            {
                yield return baseType;
                baseType = baseType.ContainingType;
            }
        }

        public IEnumerable<IPropertySymbol> GetProperties(bool includeInherited = true)
        {
            var propertyNames = new HashSet<string>();

            foreach (
                var currentType in includeInherited ? type.GetBaseTypes().Prepend(type) : [type]
            )
            {
                foreach (var property in currentType.GetMembers().OfType<IPropertySymbol>())
                {
                    if (propertyNames.Add(property.Name))
                        yield return property;
                }
            }
        }

        public ITypeSymbol? TryGetEnumerableUnderlyingType() =>
            type
                .AllInterfaces.FirstOrDefault(i =>
                    i.ConstructedFrom.SpecialType
                    == SpecialType.System_Collections_Generic_IEnumerable_T
                )
                ?.TypeArguments[0];

        public Accessibility GetActualAccessibility()
        {
            var accessibility = type.DeclaredAccessibility;

            foreach (var currentType in type.GetContainingTypes())
            {
                if (currentType.DeclaredAccessibility < accessibility)
                    accessibility = currentType.DeclaredAccessibility;
            }

            return accessibility;
        }
    }

    extension(IPropertySymbol property)
    {
        public bool IsRequired() =>
            property
                .DeclaringSyntaxReferences.Select(r => r.GetSyntax())
                .OfType<PropertyDeclarationSyntax>()
                .SelectMany(p => p.Modifiers)
                .Any(m => m.IsKind(SyntaxKind.RequiredKeyword));
    }
}
