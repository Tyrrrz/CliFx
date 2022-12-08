using System;
using System.Collections.Generic;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

internal partial class ParameterSchema : IMemberSchema
{
    public IPropertyDescriptor Property { get; }

    public int Order { get; }

    public string Name { get; }

    public bool IsRequired { get; }

    public string? Description { get; }

    public Type? ConverterType { get; }

    public IReadOnlyList<Type> ValidatorTypes { get; }

    public ParameterSchema(
        IPropertyDescriptor property,
        int order,
        string name,
        bool isRequired,
        string? description,
        Type? converterType,
        IReadOnlyList<Type> validatorTypes)
    {
        Property = property;
        Order = order;
        Name = name;
        IsRequired = isRequired;
        Description = description;
        ConverterType = converterType;
        ValidatorTypes = validatorTypes;
    }

    public string GetFormattedIdentifier() => Property.IsScalar()
        ? $"<{Name}>"
        : $"<{Name}...>";
}

internal partial class ParameterSchema
{
    public static ParameterSchema? TryResolve(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<CommandParameterAttribute>();
        if (attribute is null)
            return null;

        var name = attribute.Name?.Trim() ?? property.Name.ToLowerInvariant();
        var isRequired = attribute.IsRequired || property.IsRequired();
        var description = attribute.Description?.Trim();

        return new ParameterSchema(
            new BindablePropertyDescriptor(property),
            attribute.Order,
            name,
            isRequired,
            description,
            attribute.Converter,
            attribute.Validators
        );
    }
}