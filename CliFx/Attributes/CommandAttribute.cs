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
        /// <remarks>
        /// A command with no name is treated as a default command, i.e. it will get executed if
        /// the user does not specify the command name in the arguments.
        ///
        /// All commands registered in an application must have different names.
        /// Only one command without a name is allowed in an application.
        /// </remarks>
        public string? Name { get; }

        /// <summary>
        /// Command description, which is used in help text.
        /// </summary>
        public string? Description { get; set; }

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
        {
        }
    }
}