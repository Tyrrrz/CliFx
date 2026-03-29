using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CliFx.Generators.Utils.Extensions;

internal static class RoslynExtensions
{
    private static readonly SymbolDisplayFormat FullyQualifiedFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.AddMemberOptions(
            SymbolDisplayMemberOptions.IncludeContainingType
        );

    private static readonly SymbolDisplayFormat FullyQualifiedFormatWithGlobalPrefix =
        FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Included);

    private static readonly SymbolDisplayFormat FullyQualifiedFormatWithoutGlobalPrefix =
        FullyQualifiedFormat.WithGlobalNamespaceStyle(
            SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining
        );

    private static readonly SymbolDisplayFormat FullyQualifiedFormatWithoutGlobalPrefixOrGenerics =
        FullyQualifiedFormatWithoutGlobalPrefix.WithGenericsOptions(
            SymbolDisplayGenericsOptions.None
        );

    extension<T>(IncrementalValuesProvider<T?> source)
        where T : class
    {
        public IncrementalValuesProvider<T> WhereNotNull() =>
            source.Where(d => d is not null).Select((d, _) => d);
    }

    extension(TypeDeclarationSyntax declaration)
    {
        public IEnumerable<TypeDeclarationSyntax> GetContainingDeclarations()
        {
            var current = declaration.Parent;
            while (current is not null)
            {
                if (current is TypeDeclarationSyntax containingDeclaration)
                    yield return containingDeclaration;

                current = current.Parent;
            }
        }

        public IEnumerable<TypeDeclarationSyntax> GetSelfAndContainingDeclarations() =>
            declaration.GetContainingDeclarations().Prepend(declaration);

        public bool IsFullyPartial() =>
            declaration
                .GetSelfAndContainingDeclarations()
                .OfType<TypeDeclarationSyntax>()
                .All(t => t.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));
    }

    extension(ISymbol symbol)
    {
        public string GetGloballyQualifiedName() =>
            symbol.ToDisplayString(FullyQualifiedFormatWithGlobalPrefix);

        public string? TryGetNamespaceName() =>
            symbol.ContainingNamespace is { IsGlobalNamespace: false } ns
                ? ns.ToDisplayString(FullyQualifiedFormatWithoutGlobalPrefix)
                : null;
    }

    extension(ITypeSymbol type)
    {
        public bool IsMatchedBy(string fullyQualifiedName) =>
            string.Equals(
                type.ToDisplayString(FullyQualifiedFormatWithoutGlobalPrefixOrGenerics),
                fullyQualifiedName,
                StringComparison.Ordinal
            );

        public bool Implements(string interfaceFullyQualifiedName) =>
            type.AllInterfaces.Any(i => i.IsMatchedBy(interfaceFullyQualifiedName));

        public bool Inherits(string baseTypeFullyQualifiedName) =>
            type.GetBaseTypes().Any(t => t.IsMatchedBy(baseTypeFullyQualifiedName));

        public IEnumerable<TypeDeclarationSyntax> GetDeclarations() =>
            type
                .DeclaringSyntaxReferences.Select(r => r.GetSyntax())
                .OfType<TypeDeclarationSyntax>();

        public IEnumerable<ITypeSymbol> GetContainingTypes()
        {
            var containingType = type.ContainingType;
            while (containingType is not null)
            {
                yield return containingType;
                containingType = containingType.ContainingType;
            }
        }

        public IEnumerable<ITypeSymbol> GetSelfAndContainingTypes() =>
            type.GetContainingTypes().Prepend(type);

        public IEnumerable<ITypeSymbol> GetBaseTypes()
        {
            var baseType = type.BaseType;
            while (baseType is not null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }

        public IEnumerable<ITypeSymbol> GetSelfAndBaseTypes() => type.GetBaseTypes().Prepend(type);

        public IEnumerable<ISymbol> GetMembers(bool includeInherited = true)
        {
            foreach (var currentType in includeInherited ? type.GetSelfAndBaseTypes() : [type])
            {
                foreach (var member in currentType.GetMembers())
                {
                    yield return member;
                }
            }
        }

        public IEnumerable<IPropertySymbol> GetProperties(bool includeInherited = true) =>
            type.GetMembers(includeInherited)
                .OfType<IPropertySymbol>()
                .DistinctBy(p => p.Name, StringComparer.Ordinal);

        public IEnumerable<IMethodSymbol> GetMethods(bool includeInherited = true) =>
            type.GetMembers(includeInherited).OfType<IMethodSymbol>();

        public Accessibility GetActualAccessibility() =>
            type.GetSelfAndContainingTypes().Min(t => t.DeclaredAccessibility);

        public ITypeSymbol? TryGetEnumerableUnderlyingType() =>
            type
                .AllInterfaces.FirstOrDefault(i =>
                    i.ConstructedFrom.SpecialType
                    == SpecialType.System_Collections_Generic_IEnumerable_T
                )
                ?.TypeArguments[0];

        public ITypeSymbol? TryGetNullableUnderlyingType() =>
            type is INamedTypeSymbol { IsValueType: true } named
            && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T
                ? named.TypeArguments[0]
                : null;
    }
}
