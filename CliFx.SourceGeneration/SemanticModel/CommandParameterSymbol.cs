using System;
using System.Collections.Generic;

namespace CliFx.SourceGeneration.SemanticModel;

internal partial class CommandParameterSymbol(
    PropertyDescriptor property,
    bool isSequence,
    int order,
    string name,
    bool isRequired,
    string? description,
    TypeDescriptor? converterType,
    IReadOnlyList<TypeDescriptor> validatorTypes
) : CommandInputSymbol(property, isSequence, description, converterType, validatorTypes)
{
    public int Order { get; } = order;

    public string Name { get; } = name;

    public bool IsRequired { get; } = isRequired;
}

internal partial class CommandParameterSymbol : IEquatable<CommandParameterSymbol>
{
    public bool Equals(CommandParameterSymbol? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return base.Equals(other)
            && Order == other.Order
            && Name == other.Name
            && IsRequired == other.IsRequired;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((CommandParameterSymbol)obj);
    }

    public override int GetHashCode() =>
        HashCode.Combine(base.GetHashCode(), Order, Name, IsRequired);
}
