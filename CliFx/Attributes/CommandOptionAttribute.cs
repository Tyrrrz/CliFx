using System;

namespace CliFx.Attributes
{
    /// <summary>
    /// Annotates a property that defines a command option.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandOptionAttribute : Attribute
    {
        /// <summary>
        /// Option name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Option short name.
        /// </summary>
        public char? ShortName { get; }

        /// <summary>
        /// Option group name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Whether an option is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Option description, which is used in help text.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionAttribute"/>.
        /// </summary>
        public CommandOptionAttribute(string name, char? shortName)
        {
            Name = name; // can be null
            ShortName = shortName; // can be null
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionAttribute"/>.
        /// </summary>
        public CommandOptionAttribute(string name, char shortName)
            : this(name, (char?) shortName)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionAttribute"/>.
        /// </summary>
        public CommandOptionAttribute(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionAttribute"/>.
        /// </summary>
        public CommandOptionAttribute(char shortName)
            : this(null, shortName)
        {
        }
    }
}