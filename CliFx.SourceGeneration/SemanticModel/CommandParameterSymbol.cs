using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.SourceGeneration.Utils.Extensions;
using Microsoft.CodeAnalysis;

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

internal partial class CommandParameterSymbol
{
    public static CommandParameterSymbol FromSymbol(
        IPropertySymbol property,
        AttributeData attribute
    ) =>
        new(
            PropertyDescriptor.FromSymbol(property),
            IsSequenceType(property.Type),
            (int)attribute.ConstructorArguments.First().Value!,
            attribute.GetNamedArgumentValue("Name", default(string)),
            attribute.GetNamedArgumentValue("IsRequired", true),
            attribute.GetNamedArgumentValue("Description", default(string)),
            TypeDescriptor.FromSymbol(attribute.GetNamedArgumentValue<ITypeSymbol>("Converter")),
            attribute
                .GetNamedArgumentValues<ITypeSymbol>("Validators")
                .Select(TypeDescriptor.FromSymbol)
                .ToArray()
        );
}
