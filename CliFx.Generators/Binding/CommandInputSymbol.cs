using System.Collections.Generic;
using System.Linq;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.Binding;

internal abstract partial record CommandInputSymbol(
    IPropertySymbol Property,
    bool IsRequired,
    string? Description,
    INamedTypeSymbol? ConverterType,
    IReadOnlyList<INamedTypeSymbol> ValidatorTypes
)
{
    // Technically, we should be checking if IInputConverter.CanConvertSequence is true,
    // but we can't do that during compile time, so this is as close as we can get.
    public bool IsConverterSequenceBased =>
        ConverterType is not null
        && ConverterType.Inherits("CliFx.Activation.SequenceInputConverter");

    // An input is considered sequence-based if it has a sequence-based converter, or if it
    // doesn't have a converter but its type implements IEnumerable (except string).
    public bool IsSequenceBased =>
        ConverterType is not null
            ? IsConverterSequenceBased
            : Property.Type.SpecialType != SpecialType.System_String
                && Property.Type.Implements("System.Collections.IEnumerable");
}

internal partial record CommandInputSymbol
{
    protected static INamedTypeSymbol? TryResolveConverterType(AttributeData attribute) =>
        attribute.NamedArguments.FirstOrDefault(a => a.Key == "Converter").Value.Value
        as INamedTypeSymbol;

    protected static INamedTypeSymbol[] ResolveValidatorTypes(AttributeData attribute) =>
        attribute
            .NamedArguments.Where(a => a.Key == "Validators")
            .SelectMany(a => a.Value.Values)
            .Select(v => v.Value as INamedTypeSymbol)
            .WhereNotNull()
            .ToArray();
}
