using CliFx.SourceGeneration.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal record TypeDescriptor(ITypeSymbol Symbol)
{
    public Accessibility ActualAccessibility => Symbol.GetActualAccessibility();

    public string Name { get; } =
        Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    public string GlobalFullyQualifiedName { get; } =
        Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public string FullyQualifiedName { get; } =
        Symbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining
            )
        );

    public override string ToString() => GlobalFullyQualifiedName;
}
