using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal sealed record class ResolvedTypeIdentifier(
    INamedTypeSymbol Symbol,
    string? Namespace,
    string FullyQualifiedName,
    string Name
) : TypeIdentifier(Namespace, FullyQualifiedName, Name)
{
    // Prevent the compiler from overriding this with record semantics
    public override string ToString() => base.ToString();

    public static ResolvedTypeIdentifier From(INamedTypeSymbol symbol)
    {
        return new(
            symbol,
            symbol.ContainingNamespace is { IsGlobalNamespace: false } ns
                ? ns.ToDisplayString(FullyQualifiedFormatWithoutGlobalPrefix)
                : null,
            symbol.ToDisplayString(FullyQualifiedFormatWithoutGlobalPrefix),
            symbol.Name
        );
    }
}
