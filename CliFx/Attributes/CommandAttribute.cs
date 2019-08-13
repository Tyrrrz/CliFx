using System;

namespace CliFx.Attributes
{
    /// <summary>
    /// Annotates a type that defines a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Command name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Command description, which is used in help text.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandAttribute"/>.
        /// </summary>
        public CommandAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandAttribute"/>.
        /// </summary>
        public CommandAttribute()
            : this(null)
        {
        }
    }
}