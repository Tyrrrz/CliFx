using System.Collections.Generic;

namespace CliFx.SourceGeneration.SemanticModel;

internal record CommandOptionDescriptor(
    PropertyDescriptor Property,
    string? Name,
    char? ShortName,
    string? EnvironmentVariable,
    bool IsRequired,
    string? Description,
    TypeDescriptor? ConverterType,
    IReadOnlyList<TypeDescriptor> ValidatorTypes
);
