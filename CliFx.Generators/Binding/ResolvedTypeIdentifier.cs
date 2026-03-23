using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal class ResolvedTypeIdentifier(string fullyQualifiedName, INamedTypeSymbol symbol)
    : TypeIdentifier(fullyQualifiedName)
{
    public INamedTypeSymbol Symbol { get; } = symbol;

    public static ResolvedTypeIdentifier From(INamedTypeSymbol symbol) =>
        new(symbol.ToDisplayString(FullyQualifiedFormatWithoutGlobalPrefix), symbol);
}
