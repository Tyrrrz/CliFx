using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CliFx.SourceGeneration.Utils.Extensions;

internal static class RoslynExtensions
{
    public static bool DisplayNameMatches(this ISymbol symbol, string name) =>
        string.Equals(
            symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
            name,
            StringComparison.Ordinal
        );

    public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this ITypeSymbol type)
    {
        var current = type.BaseType;

        while (current is not null)
        {
            yield return current;
            current = current.BaseType;
        }
    }

    public static ITypeSymbol? TryGetEnumerableUnderlyingType(this ITypeSymbol type) =>
        type
            .AllInterfaces.FirstOrDefault(i =>
                i.ConstructedFrom.SpecialType
                == SpecialType.System_Collections_Generic_IEnumerable_T
            )
            ?.TypeArguments[0];

    public static bool IsRequired(this IPropertySymbol property) =>
        property
            .DeclaringSyntaxReferences.Select(r => r.GetSyntax())
            .OfType<PropertyDeclarationSyntax>()
            .SelectMany(p => p.Modifiers)
            // SyntaxKind.RequiredKeyword = 8447
            .Any(m => m.IsKind((SyntaxKind)8447));

    public static bool ImplementsInterface(this ITypeSymbol type, string interfaceName) =>
        type.AllInterfaces.Any(i => i.DisplayNameMatches(interfaceName))
        || type.DisplayNameMatches(interfaceName);

    public static bool InheritsFrom(this ITypeSymbol type, string baseTypeName) =>
        type.GetBaseTypes().Any(b => b.ConstructedFrom.DisplayNameMatches(baseTypeName));
}
