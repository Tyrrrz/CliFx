using System.Collections.Generic;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal abstract record CommandInputSymbol(
    IPropertySymbol Property,
    bool IsRequired,
    string? Description,
    TypeIdentifier? ConverterType,
    IReadOnlyList<TypeIdentifier> ValidatorTypes
)
{
    public bool IsSequence { get; } =
        Property.Type.SpecialType != SpecialType.System_String
        && Property.Type.TryGetEnumerableUnderlyingType() is not null;
}
