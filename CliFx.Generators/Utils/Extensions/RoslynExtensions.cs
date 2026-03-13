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
        public IEnumerable<IPropertySymbol> GetProperties(bool includeInherited = true)
        {
            var propertyNames = new HashSet<string>();
            var containingType = type;

            while (
                containingType is not null
                && containingType.SpecialType != SpecialType.System_Object
            )
            {
                foreach (var property in containingType.GetMembers().OfType<IPropertySymbol>())
                {
                    if (propertyNames.Add(property.Name))
                        yield return property;
                }

                if (includeInherited)
                    containingType = containingType.BaseType;
                else
                    yield break;
            }
        }

        public ITypeSymbol? TryGetEnumerableUnderlyingType() =>
            type
                .AllInterfaces.FirstOrDefault(i =>
                    i.ConstructedFrom.SpecialType
                    == SpecialType.System_Collections_Generic_IEnumerable_T
                )
                ?.TypeArguments[0];

        public bool ImplementsInterface(string interfaceName) =>
            type.AllInterfaces.Any(i => i.DisplayNameMatches(interfaceName))
            || type.DisplayNameMatches(interfaceName);

        /// <summary>
        /// Returns the effective accessibility of the type by walking up the containment chain
        /// and returning the most restrictive accessibility encountered.
        /// </summary>
        public Accessibility GetActualAccessibility()
        {
            var current = type;
            var accessibility = Accessibility.Public;

            while (current is not null)
            {
                if (current.DeclaredAccessibility < accessibility)
                    accessibility = current.DeclaredAccessibility;
                current = current.ContainingType;
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
