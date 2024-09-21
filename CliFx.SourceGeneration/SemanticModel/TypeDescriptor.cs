using System;
using CliFx.SourceGeneration.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal partial class TypeDescriptor(string fullyQualifiedName)
{
    public string FullyQualifiedName { get; } = fullyQualifiedName;

    public string Namespace { get; } = fullyQualifiedName.SubstringUntilLast(".");

    public string Name { get; } = fullyQualifiedName.SubstringAfterLast(".");
}

internal partial class TypeDescriptor : IEquatable<TypeDescriptor>
{
    public bool Equals(TypeDescriptor? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return FullyQualifiedName == other.FullyQualifiedName;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((TypeDescriptor)obj);
    }

    public override int GetHashCode() => FullyQualifiedName.GetHashCode();
}

internal partial class TypeDescriptor
{
    public static TypeDescriptor FromSymbol(ITypeSymbol symbol) =>
        new(symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
}
