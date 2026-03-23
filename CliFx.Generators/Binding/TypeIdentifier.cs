using System;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal partial class TypeIdentifier(string fullyQualifiedName)
{
    public string? Namespace { get; } =
        fullyQualifiedName.LastIndexOf('.') is int pos && pos > 0
            ? fullyQualifiedName[..pos]
            : null;

    public string Name { get; } =
        fullyQualifiedName.LastIndexOf('.') is int pos && pos > 0
            ? fullyQualifiedName[(pos + 1)..]
            : fullyQualifiedName;

    public string FullyQualifiedName { get; } = fullyQualifiedName;

    public string GlobalFullyQualifiedName { get; } = "global::" + fullyQualifiedName;

    public bool IsMatchedBy(INamedTypeSymbol symbol) =>
        string.Equals(
            symbol.ToDisplayString(FullyQualifiedFormatWithoutGlobalPrefix),
            FullyQualifiedName,
            StringComparison.Ordinal
        );

    public override string ToString() => GlobalFullyQualifiedName;
}

internal partial class TypeIdentifier
{
    internal static readonly SymbolDisplayFormat FullyQualifiedFormatWithoutGlobalPrefix =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
            SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining
        );

    public static TypeIdentifier From(ITypeSymbol symbol) =>
        new(symbol.ToDisplayString(FullyQualifiedFormatWithoutGlobalPrefix));
}
