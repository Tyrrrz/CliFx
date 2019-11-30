using System;

namespace CliFx.Attributes
{
    /// <summary>
    /// Annotates a property that defines a command argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandArgumentAttribute : Attribute
    {
        /// <summary>
        /// The name of the argument, which is used in help text.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Whether the argument is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Argument description, which is used in help text.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The ordering of the argument. Lower values will appear before higher values.
        /// <remarks>
        /// Two arguments of the same command cannot have the same <see cref="Order"/>.
        /// </remarks>
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandArgumentAttribute"/> with a given order.
        /// </summary>
        public CommandArgumentAttribute(int order)
        {
            Order = order;
        }
    }
}