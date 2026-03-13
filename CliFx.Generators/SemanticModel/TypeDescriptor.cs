using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.SemanticModel;

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

    // No generic parameters
    public string GlobalBaseFullyQualifiedName { get; } =
        Symbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat.WithGenericsOptions(
                SymbolDisplayGenericsOptions.None
            )
        );

    public override string ToString() => GlobalFullyQualifiedName;
}
