using CliFx.SourceGeneration.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal record TypeDescriptor(ITypeSymbol Symbol)
{
    private static readonly SymbolDisplayFormat GlobalNoTypeArgsFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGenericsOptions(
            SymbolDisplayGenericsOptions.None
        );

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

    /// <summary>
    /// Like <see cref="GlobalFullyQualifiedName"/> but with no generic type-argument
    /// placeholders — e.g. <c>global::Ns.EnumBindingConverter</c> instead of
    /// <c>global::Ns.EnumBindingConverter&lt;T&gt;</c>.
    /// For non-generic types this is identical to <see cref="ToString"/>.
    /// Use this when emitting a generic instantiation with concrete type arguments.
    /// </summary>
    public string GlobalBase { get; } = Symbol.ToDisplayString(GlobalNoTypeArgsFormat);

    public override string ToString() => GlobalFullyQualifiedName;
}
