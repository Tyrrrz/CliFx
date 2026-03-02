using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal record CommandOptionDescriptor(
    IPropertySymbol Property,
    string? Name,
    char? ShortName,
    string? EnvironmentVariable,
    bool IsRequired,
    string? Description,
    TypeDescriptor? ConverterType,
    IReadOnlyList<TypeDescriptor> ValidatorTypes
);
