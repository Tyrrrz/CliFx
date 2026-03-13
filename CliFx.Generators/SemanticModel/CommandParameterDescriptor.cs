using System.Collections.Generic;

namespace CliFx.Generators.SemanticModel;

internal record CommandParameterDescriptor(
    PropertyDescriptor Property,
    int Order,
    string Name,
    string? Description,
    TypeDescriptor? ConverterType,
    IReadOnlyList<TypeDescriptor> ValidatorTypes
);
