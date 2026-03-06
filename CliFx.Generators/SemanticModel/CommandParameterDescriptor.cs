using System.Collections.Generic;

namespace CliFx.SourceGeneration.SemanticModel;

internal record CommandParameterDescriptor(
    PropertyDescriptor Property,
    int Order,
    string Name,
    bool IsRequired,
    string? Description,
    TypeDescriptor? ConverterType,
    IReadOnlyList<TypeDescriptor> ValidatorTypes
);
