using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal class TypeDescriptor
{
    public string FullyQualifiedName { get; }
    public bool IsNullable { get; }
    public bool IsSequence { get; }

    public TypeDescriptor(
        string fullyQualifiedName,
        bool isNullable = false,
        bool isSequence = false
    )
    {
        FullyQualifiedName = fullyQualifiedName;
        IsNullable = isNullable;
        IsSequence = isSequence;
    }

    public static TypeDescriptor FromSymbol(ITypeSymbol symbol)
    {
        var fqn = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return new TypeDescriptor(fqn);
    }

    public static TypeDescriptor? TryFromSymbol(ITypeSymbol? symbol) =>
        symbol is not null ? FromSymbol(symbol) : null;
}
