using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.SemanticModel;

internal record PropertyDescriptor(IPropertySymbol Symbol)
{
    public bool IsRequired { get; } = Symbol.IsRequired();

    public TypeDescriptor Type { get; } = new(Symbol.Type);

    public string Name { get; } = Symbol.Name;

    // Excludes string, which implements IEnumerable<char> but is not a binding sequence type.
    public bool IsSequenceType { get; } =
        Symbol.Type.SpecialType != SpecialType.System_String
        && Symbol.Type.TryGetEnumerableUnderlyingType() is not null;

    public override string ToString() => $"{Type} {Name}";
}
