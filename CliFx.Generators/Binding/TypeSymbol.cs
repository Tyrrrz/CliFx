using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal record TypeSymbol(ITypeSymbol Symbol)
{
    public Accessibility ActualAccessibility { get; } = Symbol.GetActualAccessibility();

    public string Name { get; } =
        Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    public string FullyQualifiedName { get; } =
        Symbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining
            )
        );

    public string GlobalFullyQualifiedName { get; } =
        Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    // No generic parameters
    public string GlobalBaseFullyQualifiedName { get; } =
        Symbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat.WithGenericsOptions(
                SymbolDisplayGenericsOptions.None
            )
        );

    public override string ToString() => GlobalFullyQualifiedName;
}
