using System;

namespace CliFx.Attributes
{
    /// <summary>
    /// Annotates a property that defines a command parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandParameterAttribute : CommandMemberAttribute
    {
        /// <summary>
        /// Parameter's order.
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
        /// Parameter's name.
        /// This is shown to the user in the help text.
        /// </summary>
        /// <remarks>
        /// If this isn't specified, parameter name is inferred from the property name.
        /// </remarks>
        public string? Name { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandParameterAttribute"/>.
        /// </summary>
        public CommandParameterAttribute(int order)
        {
            Order = order;
        }
    }
}