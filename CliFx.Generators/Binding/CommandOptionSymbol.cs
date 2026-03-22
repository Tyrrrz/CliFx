using System.Collections.Generic;

namespace CliFx.Generators.Binding;

internal record CommandOptionSymbol(
    PropertySymbol Property,
    string? Name,
    char? ShortName,
    string? EnvironmentVariable,
    bool IsRequired,
    string? Description,
    TypeSymbol? ConverterType,
    IReadOnlyList<TypeSymbol> ValidatorTypes
) : CommandInputSymbol(Property, IsRequired, Description, ConverterType, ValidatorTypes);
