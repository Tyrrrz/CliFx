using System;

namespace CliFx.Attributes
{
    /// <summary>
    /// Annotates a property that defines a command option.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandOptionAttribute : CommandArgumentAttribute
    {
        /// <summary>
        /// Option name.
        /// </summary>
        /// <remarks>
        /// Must contain at least two characters and start with a letter.
        /// Either <see cref="Name"/> or <see cref="ShortName"/> must be set.
        /// All options in a command must have different names (comparison is not case-sensitive).
        /// </remarks>
        public string? Name { get; }

        /// <summary>
        /// Option short name.
        /// </summary>
        /// <remarks>
        /// Either <see cref="Name"/> or <see cref="ShortName"/> must be set.
        /// All options in a command must have different short names (comparison is case-sensitive).
        /// </remarks>
        public char? ShortName { get; }

        /// <summary>
        /// Whether the option is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Environment variable that will be used as fallback if
        /// the value isn't explicitly specified in the arguments.
        /// </summary>
        public string? EnvironmentVariableName { get; set; }

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