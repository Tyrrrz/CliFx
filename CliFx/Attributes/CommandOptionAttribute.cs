using System;

namespace CliFx.Attributes
{
    /// <summary>
    /// Annotates a property that defines a command option.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandOptionAttribute : CommandMemberAttribute
    {
        /// <summary>
        /// Option's name.
        /// </summary>
        /// <remarks>
        /// Must contain at least two characters and start with a letter.
        /// Either <see cref="Name"/> or <see cref="ShortName"/> must be set.
        /// All options in a command must have unique names (comparison IS NOT case-sensitive).
        /// </remarks>
        public string? Name { get; }

        /// <summary>
        /// Option's short name.
        /// </summary>
        /// <remarks>
        /// Either <see cref="Name"/> or <see cref="ShortName"/> must be set.
        /// All options in a command must have unique short names (comparison IS case-sensitive).
        /// </remarks>
        public char? ShortName { get; }

        /// <summary>
        /// Whether this option is required.
        /// If an option is required, the user will get an error if they don't set it.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Environment variable whose value will be used as a fallback if the option
        /// has not been explicitly set through command line arguments.
        /// </summary>
        public string? EnvironmentVariable { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionAttribute"/>.
        /// </summary>
        private CommandOptionAttribute(string? name, char? shortName)
        {
            Name = name;
            ShortName = shortName;
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
            : this(null, (char?) shortName)
        {
        }
    }
}