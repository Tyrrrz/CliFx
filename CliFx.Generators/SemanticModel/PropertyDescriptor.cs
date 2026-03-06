using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal record PropertyDescriptor(IPropertySymbol Symbol)
{
    public TypeDescriptor Type { get; } = new(Symbol.Type);

    public string Name { get; } = Symbol.Name;

    public override string ToString() => $"{Type} {Name}";
}
