using System;
using System.Collections.Generic;

namespace CliFx.Attributes;

/// <summary>
/// Binds a property to a command parameter — a command-line input that is identified by its relative position (order).
/// Higher order means that the parameter appears later, lower order means that it appears earlier.
/// </summary>
/// <remarks>
/// All parameters in a command must have unique order values.
/// If a parameter is bound to a property whose type is a sequence (i.e. implements <see cref="IEnumerable{T}"/>; except <see cref="string" />),
/// then it must have the highest order in the command.
/// Only one sequential parameter is allowed per command.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class CommandParameterAttribute(int order) : CommandInputAttribute
{
    /// <summary>
    /// Parameter order.
    /// </summary>
    public int Order { get; } = order;

    /// <summary>
    /// Whether this parameter is required (default: <c>true</c>).
    /// If a parameter is required, the user will get an error when they don't set it.
    /// </summary>
    /// <remarks>
    /// Parameter marked as non-required must have the highest order in the command.
    /// Only one non-required parameter is allowed per command.
    /// </remarks>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Parameter name, as shown in the help text.
    /// </summary>
    /// <remarks>
    /// If this isn't specified, parameter name is inferred from the property name.
    /// </remarks>
    public string? Name { get; set; }
}
