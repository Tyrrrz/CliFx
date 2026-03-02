using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal class CommandParameterDescriptor(
    IPropertySymbol property,
    int order,
    string name,
    bool isRequired,
    string? description,
    TypeDescriptor? converterType,
    IReadOnlyList<TypeDescriptor> validatorTypes
)
{
    public IPropertySymbol Property { get; } = property;
    public int Order { get; } = order;
    public string Name { get; } = name;
    public bool IsRequired { get; } = isRequired;
    public string? Description { get; } = description;
    public TypeDescriptor? ConverterType { get; } = converterType;
    public IReadOnlyList<TypeDescriptor> ValidatorTypes { get; } = validatorTypes;
}
