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
        /// The ordering of the argument. Lower values will appear before higher values. If no order is given, it will be placed last.
        ///
        /// <remarks>
        /// If two arguments have the same value for <see cref="Order"/>, they will be ordered according to their <see cref="Name"/> properties, or the property name if <see cref="Name"/> is not set.
        /// </remarks>
        /// </summary>
        public int Order { get; set; } = int.MaxValue;
    }
}