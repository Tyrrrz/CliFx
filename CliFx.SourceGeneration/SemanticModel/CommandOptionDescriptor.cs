using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal class CommandOptionDescriptor(
    IPropertySymbol property,
    string? name,
    char? shortName,
    string? environmentVariable,
    bool isRequired,
    string? description,
    TypeDescriptor? converterType,
    IReadOnlyList<TypeDescriptor> validatorTypes
)
{
    public IPropertySymbol Property { get; } = property;
    public string? Name { get; } = name;
    public char? ShortName { get; } = shortName;
    public string? EnvironmentVariable { get; } = environmentVariable;
    public bool IsRequired { get; } = isRequired;
    public string? Description { get; } = description;
    public TypeDescriptor? ConverterType { get; } = converterType;
    public IReadOnlyList<TypeDescriptor> ValidatorTypes { get; } = validatorTypes;
}
