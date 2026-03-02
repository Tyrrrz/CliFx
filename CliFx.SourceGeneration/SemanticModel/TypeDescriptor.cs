using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal record TypeDescriptor(string FullyQualifiedName)
{
    public static TypeDescriptor FromSymbol(ITypeSymbol symbol) =>
        new(symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

    public static TypeDescriptor? TryFromSymbol(ITypeSymbol? symbol) =>
        symbol is not null ? FromSymbol(symbol) : null;
}
