using System.Collections.Generic;
using System.Text;
using CliFx.Activation;

namespace CliFx.Binding;

/// <summary>
/// Describes a binding between a CLR property and a parameter input of a command.
/// </summary>
public class CommandParameterDescriptor(
    PropertyDescriptor property,
    int order,
    string name,
    bool isRequired,
    string? description,
    IInputConverter converter,
    IReadOnlyList<IInputValidator> validators
) : CommandInputDescriptor(property, isRequired, description, converter, validators)
{
    /// <inheritdoc cref="CommandParameterAttribute.Order" />
    public int Order { get; } = order;

    /// <inheritdoc cref="CommandParameterAttribute.Name" />
    public string Name { get; } = name;

    /// <inheritdoc cref="ToString()" />
    public string ToString(bool includeKind)
    {
        var buffer = new StringBuilder();

        if (includeKind)
            buffer.Append("Parameter ");

        buffer.Append(Converter.IsSequence ? $"<{Name}...>" : $"<{Name}>");

        return buffer.ToString();
    }

    /// <inheritdoc />
    public override string ToString() => ToString(true);
}

/// <inheritdoc cref="CommandParameterDescriptor" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public class CommandParameterDescriptor<TCommand, TProperty>(
    PropertyDescriptor<TCommand, TProperty> property,
    int order,
    string name,
    bool isRequired,
    string? description,
    InputConverter<TProperty> converter,
    IReadOnlyList<InputValidator<TProperty>> validators
)
    : CommandParameterDescriptor(
        property,
        order,
        name,
        isRequired,
        description,
        converter,
        validators
    )
    where TCommand : ICommand;
