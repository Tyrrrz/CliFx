using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CliFx.Generators.Utils.Extensions;

internal static class RoslynExtensions
{
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

    extension(ITypeSymbol type)
    {
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
            var memberNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var currentType in includeInherited ? type.GetSelfAndBaseTypes() : [type])
            {
                foreach (var member in currentType.GetMembers())
                {
                    if (memberNames.Add(member.Name))
                        yield return member;
                }
            }
        }

        public IEnumerable<IPropertySymbol> GetProperties(bool includeInherited = true) =>
            type.GetMembers(includeInherited).OfType<IPropertySymbol>();

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

        public ITypeSymbol? TryGetEnumerableUnderlyingType() =>
            type
                .AllInterfaces.FirstOrDefault(i =>
                    i.ConstructedFrom.SpecialType
                    == SpecialType.System_Collections_Generic_IEnumerable_T
                )
                ?.TypeArguments[0];
    }

    extension(IPropertySymbol property)
    {
        public IEnumerable<PropertyDeclarationSyntax> GetDeclarations() =>
            property
                .DeclaringSyntaxReferences.Select(r => r.GetSyntax())
                .OfType<PropertyDeclarationSyntax>();

        public bool IsRequired() =>
            property
                .GetDeclarations()
                .SelectMany(p => p.Modifiers)
                .Any(m => m.IsKind(SyntaxKind.RequiredKeyword));
    }
}
