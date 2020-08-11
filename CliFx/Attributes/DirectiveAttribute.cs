using System;

namespace CliFx.Attributes
{
    /// <summary>
    /// Annotates a type that defines a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DirectiveAttribute : Attribute
    {
        /// <summary>
        /// Directive name.
        /// All directives in an application must have different names.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Directive description, which is used in help text.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether directive can run only in interactive mode.
        /// </summary>
        public bool InteractiveModeOnly { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandAttribute"/>.
        /// </summary>
        public DirectiveAttribute(string name)
        {
            Name = name;
        }
    }
}