using System;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.SourceGeneration.SemanticModel;

internal partial class CommandSymbol(
    TypeDescriptor type,
    string? name,
    string? description,
    IReadOnlyList<CommandInputSymbol> inputs
)
{
    public TypeDescriptor Type { get; } = type;

    public string? Name { get; } = name;

    public string? Description { get; } = description;

    public IReadOnlyList<CommandInputSymbol> Inputs { get; } = inputs;

    public IReadOnlyList<CommandParameterSymbol> Parameters =>
        Inputs.OfType<CommandParameterSymbol>().ToArray();

    public IReadOnlyList<CommandOptionSymbol> Options =>
        Inputs.OfType<CommandOptionSymbol>().ToArray();
}

internal partial class CommandSymbol : IEquatable<CommandSymbol>
{
    public bool Equals(CommandSymbol? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Type.Equals(other.Type)
            && Name == other.Name
            && Description == other.Description
            && Inputs.SequenceEqual(other.Inputs);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((CommandSymbol)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Type, Name, Description, Inputs);
}
