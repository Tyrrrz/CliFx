using System;
using CliFx.Extensibility;

namespace CliFx.Attributes
{
    /// <summary>
    /// Properties shared by parameter and option attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class CommandArgumentAttribute : Attribute
    {
        /// <summary>
        /// Option description, which is used in help text.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Type of converter to use when mapping the argument value.
        /// Converter must derive from <see cref="ArgumentValueConverter{T}"/>.
        /// </summary>
        public Type? Converter { get; set; }

        /// <summary>
        /// Types of validators to use when mapping the argument value.
        /// Validators must derive from <see cref="ArgumentValueValidator{T}"/>.
        /// </summary>
        public Type[] Validators { get; set; } = Array.Empty<Type>();
    }
}