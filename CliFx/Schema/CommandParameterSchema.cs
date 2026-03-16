using System.Collections.Generic;
using CliFx.Infrastructure.Binding;

namespace CliFx.Schema;

/// <summary>
/// Describes a parameter input of a command.
/// </summary>
public class CommandParameterSchema(
    PropertyBinding property,
    int order,
    string name,
    bool isRequired,
    string? description,
    IBindingConverter converter,
    IReadOnlyList<IBindingValidator> validators
) : CommandInputSchema(property, isRequired, description, converter, validators)
{
    /// <summary>
    /// Position order of this parameter.
    /// </summary>
    public int Order { get; } = order;

    /// <summary>
    /// Parameter name, shown in the help text.
    /// </summary>
    public string Name { get; } = name;
}

/// <inheritdoc cref="CommandParameterSchema" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public class CommandParameterSchema<TCommand, TProperty>(
    PropertyBinding<TCommand, TProperty> property,
    int order,
    string name,
    bool isRequired,
    string? description,
    BindingConverter<TProperty> converter,
    IReadOnlyList<BindingValidator<TProperty>> validators
) : CommandParameterSchema(property, order, name, isRequired, description, converter, validators)
    where TCommand : ICommand;
