using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal abstract partial class CommandInputSymbol(
    PropertyDescriptor property,
    bool isSequence,
    string? description,
    TypeDescriptor? converterType,
    IReadOnlyList<TypeDescriptor> validatorTypes
)
{
    public PropertyDescriptor Property { get; } = property;

    public bool IsSequence { get; } = isSequence;

    public string? Description { get; } = description;

    public TypeDescriptor? ConverterType { get; } = converterType;

    public IReadOnlyList<TypeDescriptor> ValidatorTypes { get; } = validatorTypes;
}

internal partial class CommandInputSymbol : IEquatable<CommandInputSymbol>
{
    public bool Equals(CommandInputSymbol? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Property.Equals(other.Property)
            && IsSequence == other.IsSequence
            && Description == other.Description
            && Equals(ConverterType, other.ConverterType)
            && ValidatorTypes.SequenceEqual(other.ValidatorTypes);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((CommandInputSymbol)obj);
    }

    public override int GetHashCode() =>
        HashCode.Combine(Property, IsSequence, Description, ConverterType, ValidatorTypes);
}

internal partial class CommandInputSymbol
{
    public static bool IsSequenceType(ITypeSymbol type) =>
        type.AllInterfaces.Any(i =>
            i.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
        )
        && type.SpecialType != SpecialType.System_String;
}
