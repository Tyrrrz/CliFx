using System;

namespace CliFx;

/// <summary>
/// Binds a property to a parameter input of a command.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class CommandParameterAttribute(int order) : CommandInputAttribute
{
    /// <summary>
    /// Relative parameter order.
    /// Higher order means the parameter appears later, lower order means it appears earlier.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All parameters in a command must have unique order values.
    /// </para>
    /// <para>
    /// Sequence-based parameters must always appear last.
    /// Only one sequence-based parameter is allowed in a command.
    /// </para>
    /// </remarks>
    public int Order { get; } = order;

    /// <summary>
    /// Parameter name.
    /// Used for display purposes in the help text.
    /// </summary>
    /// <remarks>
    /// If this isn't specified, parameter name is inferred from the property name.
    /// </remarks>
    public string? Name { get; set; }
}
