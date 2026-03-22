using System.Collections.Generic;

namespace CliFx.Generators.Binding;

internal record CommandParameterSymbol(
    PropertySymbol Property,
    int Order,
    string Name,
    bool IsRequired,
    string? Description,
    TypeSymbol? ConverterType,
    IReadOnlyList<TypeSymbol> ValidatorTypes
) : CommandInputSymbol(Property, IsRequired, Description, ConverterType, ValidatorTypes);
