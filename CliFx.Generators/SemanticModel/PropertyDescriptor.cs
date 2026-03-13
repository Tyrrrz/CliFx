using CliFx.SourceGeneration.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal record PropertyDescriptor(IPropertySymbol Symbol)
{
    public bool IsRequired { get; } = Symbol.IsRequired();

    public TypeDescriptor Type { get; } = new(Symbol.Type);

    public string Name { get; } = Symbol.Name;

    public override string ToString() => $"{Type} {Name}";
}
