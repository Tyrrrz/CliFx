using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal record PropertySymbol(IPropertySymbol Symbol)
{
    public bool IsRequired { get; } = Symbol.IsRequired();

    public TypeSymbol Type { get; } = new(Symbol.Type);

    public string Name { get; } = Symbol.Name;

    public override string ToString() => $"{Type} {Name}";
}
