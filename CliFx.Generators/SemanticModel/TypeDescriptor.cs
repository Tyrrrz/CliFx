using System;
using CliFx.SourceGeneration.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal record TypeDescriptor(ITypeSymbol Symbol)
{
    public Accessibility ActualAccessibility => Symbol.GetActualAccessibility();

    public string Name { get; } =
        Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    public string FullyQualifiedName { get; } =
        Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) is { } fqn
        && fqn.StartsWith("global::", StringComparison.Ordinal)
            ? fqn.Substring("global::".Length)
            : Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public string GlobalFullyQualifiedName => $"global::{FullyQualifiedName}";

    public override string ToString() => GlobalFullyQualifiedName;
}
