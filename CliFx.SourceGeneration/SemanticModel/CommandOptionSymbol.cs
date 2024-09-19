using System;
using System.Collections.Generic;

namespace CliFx.SourceGeneration.SemanticModel;

internal partial class CommandOptionSymbol(
    PropertyDescriptor property,
    bool isSequence,
    string? name,
    char? shortName,
    string? environmentVariable,
    bool isRequired,
    string? description,
    TypeDescriptor? converterType,
    IReadOnlyList<TypeDescriptor> validatorTypes
) : CommandInputSymbol(property, isSequence, description, converterType, validatorTypes)
{
    public string? Name { get; } = name;

    public char? ShortName { get; } = shortName;

    public string? EnvironmentVariable { get; } = environmentVariable;

    public bool IsRequired { get; } = isRequired;
}

internal partial class CommandOptionSymbol : IEquatable<CommandOptionSymbol>
{
    public bool Equals(CommandOptionSymbol? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return base.Equals(other)
            && Name == other.Name
            && ShortName == other.ShortName
            && EnvironmentVariable == other.EnvironmentVariable
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

        return Equals((CommandOptionSymbol)obj);
    }

    public override int GetHashCode() =>
        HashCode.Combine(base.GetHashCode(), Name, ShortName, EnvironmentVariable, IsRequired);
}
