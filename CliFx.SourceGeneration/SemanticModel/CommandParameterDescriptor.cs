using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal record CommandParameterDescriptor(
    IPropertySymbol Property,
    int Order,
    string Name,
    bool IsRequired,
    string? Description,
    TypeDescriptor? ConverterType,
    IReadOnlyList<TypeDescriptor> ValidatorTypes
);
