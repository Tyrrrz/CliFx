using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CliFx.SourceGeneration.Utils.Extensions;

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
        public IEnumerable<INamedTypeSymbol> GetBaseTypes()
        {
            var current = type.BaseType;

            while (current is not null)
            {
                yield return current;
                current = current.BaseType;
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
