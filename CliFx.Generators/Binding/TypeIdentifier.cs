using System;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal partial record class TypeIdentifier(
    string? Namespace,
    string FullyQualifiedName,
    string Name
)
{
    public string GlobalFullyQualifiedName { get; } = "global::" + FullyQualifiedName;

    public bool IsMatchedBy(INamedTypeSymbol symbol) =>
        string.Equals(
            symbol.ToDisplayString(FullyQualifiedFormatWithoutGlobalPrefixOrGenerics),
            FullyQualifiedName,
            StringComparison.Ordinal
        );

    public override string ToString() => GlobalFullyQualifiedName;
}

internal partial record class TypeIdentifier
{
    internal static readonly SymbolDisplayFormat FullyQualifiedFormatWithoutGlobalPrefix =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
            SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining
        );

    private static readonly SymbolDisplayFormat FullyQualifiedFormatWithoutGlobalPrefixOrGenerics =
        FullyQualifiedFormatWithoutGlobalPrefix.WithGenericsOptions(
            SymbolDisplayGenericsOptions.None
        );

    public static TypeIdentifier From(ITypeSymbol symbol) =>
        new(
            symbol.ContainingNamespace is { IsGlobalNamespace: false } ns
                ? ns.ToDisplayString(FullyQualifiedFormatWithoutGlobalPrefix)
                : null,
            symbol.ToDisplayString(FullyQualifiedFormatWithoutGlobalPrefix),
            symbol.Name
        );
}
