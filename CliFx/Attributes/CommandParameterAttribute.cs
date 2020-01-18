using System;

namespace CliFx.Attributes
{
    /// <summary>
    /// Annotates a property that defines a command argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandParameterAttribute : Attribute
    {
        /// <summary>
        /// Order of this argument compared to other arguments.
        /// Two arguments in the same command cannot have the same order.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Argument name, which is only used in help text.
        /// If this isn't specified, property name is used instead.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Argument description, which is used in help text.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandParameterAttribute"/> with a given order.
        /// </summary>
        public CommandParameterAttribute(int order)
        {
            Order = order;
        }
    }
}