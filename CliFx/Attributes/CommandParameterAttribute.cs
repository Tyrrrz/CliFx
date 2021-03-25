using System;
using CliFx.Extensibility;

namespace CliFx.Attributes
{
    /// <summary>
    /// Annotates a property that defines a command parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CommandParameterAttribute : Attribute
    {
        /// <summary>
        /// Parameter order.
        /// </summary>
        /// <remarks>
        /// Higher order means the parameter appears later, lower order means
        /// it appears earlier.
        ///
        /// All parameters in a command must have unique order.
        ///
        /// Parameter whose type is a non-scalar (e.g. array), must always be the last in order.
        /// Only one non-scalar parameter is allowed in a command.
        /// </remarks>
        public int Order { get; }

        /// <summary>
        /// Parameter name.
        /// This is shown to the user in the help text.
        /// </summary>
        /// <remarks>
        /// If this isn't specified, parameter name is inferred from the property name.
        /// </remarks>
        public string? Name { get; set; }

        /// <summary>
        /// Parameter description.
        /// This is shown to the user in the help text.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Custom converter used for mapping the raw command line argument into
        /// a value expected by the underlying property.
        /// </summary>
        /// <remarks>
        /// Converter must derive from <see cref="BindingConverter{T}"/>.
        /// </remarks>
        public Type? Converter { get; set; }

        /// <summary>
        /// Custom validators used for verifying the value of the underlying
        /// property, after it has been bound.
        /// </summary>
        /// <remarks>
        /// Validators must derive from <see cref="BindingValidator{T}"/>.
        /// </remarks>
        public Type[] Validators { get; set; } = Array.Empty<Type>();

        /// <summary>
        /// Initializes an instance of <see cref="CommandParameterAttribute"/>.
        /// </summary>
        public CommandParameterAttribute(int order)
        {
            Order = order;
        }
    }
}