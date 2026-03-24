using System.Collections.Generic;
using System.Linq;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal abstract record CommandInputSymbol(
    IPropertySymbol Property,
    bool IsRequired,
    string? Description,
    ResolvedTypeIdentifier? ConverterType,
    IReadOnlyList<ResolvedTypeIdentifier> ValidatorTypes
)
{
    // Technically, we should be checking if IInputConverter.CanConvertSequence is true,
    // but we can't do that during compile time, so this is the best we can do.
    public bool IsConverterSequenceBased =>
        ConverterType is not null
        && ConverterType
            .Symbol.GetBaseTypes()
            .Prepend(ConverterType.Symbol)
            .OfType<INamedTypeSymbol>()
            .Any(t =>
                t.OriginalDefinition.Name == "SequenceInputConverter"
                && t.OriginalDefinition.ContainingNamespace.ToDisplayString(
                    TypeIdentifier.FullyQualifiedFormatWithoutGlobalPrefix
                ) == "CliFx.Activation"
            );

    // An input is considered sequence-based if it has a sequence-based converter, or if it
    // doesn't have a converter but its type is an enumerable (except string).
    public bool IsSequenceBased =>
        ConverterType is not null
            ? IsConverterSequenceBased
            : Property.Type.SpecialType != SpecialType.System_String
                && Property.Type.TryGetEnumerableUnderlyingType() is not null;
}
