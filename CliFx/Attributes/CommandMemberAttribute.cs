using System;
using CliFx.Extensibility;

namespace CliFx.Attributes
{
    /// <summary>
    /// Base type for <see cref="CommandParameterAttribute"/> and <see cref="CommandOptionAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class CommandMemberAttribute : Attribute
    {
        /// <summary>
        /// Option's or parameter's description.
        /// This is shown to the user in the help text.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Custom converter used for mapping the raw command line argument into
        /// a value expected by the underlying property.
        /// </summary>
        /// <remarks>
        /// Converter must derive from <see cref="ArgumentConverter{T}"/>.
        /// </remarks>
        public Type? Converter { get; set; }

        /// <summary>
        /// Custom validators used for verifying the value of the underlying
        /// property, after it has been bound.
        /// </summary>
        /// <remarks>
        /// Validators must derive from <see cref="ArgumentValidator{T}"/>.
        /// </remarks>
        public Type[] Validators { get; set; } = Array.Empty<Type>();
    }
}