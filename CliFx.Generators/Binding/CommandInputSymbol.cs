using System.Collections.Generic;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal abstract record CommandInputSymbol(
    PropertySymbol Property,
    bool IsRequired,
    string? Description,
    TypeSymbol? ConverterType,
    IReadOnlyList<TypeSymbol> ValidatorTypes
)
{
    public bool IsSequence { get; } =
        Property.Symbol.Type.SpecialType != SpecialType.System_String
        && Property.Symbol.Type.TryGetEnumerableUnderlyingType() is not null;
}
