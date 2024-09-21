using System;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal partial class PropertyDescriptor(TypeDescriptor type, string name)
{
    public TypeDescriptor Type { get; } = type;

    public string Name { get; } = name;
}

internal partial class PropertyDescriptor : IEquatable<PropertyDescriptor>
{
    public bool Equals(PropertyDescriptor? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Type.Equals(other.Type) && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((PropertyDescriptor)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Type, Name);
}

internal partial class PropertyDescriptor
{
    public static PropertyDescriptor FromSymbol(IPropertySymbol symbol) =>
        new(TypeDescriptor.FromSymbol(symbol.Type), symbol.Name);
}
