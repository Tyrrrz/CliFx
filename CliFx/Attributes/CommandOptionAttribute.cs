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
        /// Either <see cref="Name"/> or <see cref="ShortName"/> must be set.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Option short name.
        /// Either <see cref="Name"/> or <see cref="ShortName"/> must be set.
        /// </summary>
        public char? ShortName { get; }

        /// <summary>
        /// Whether an option is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Option description, which is used in help text.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Optional environment variable name that will be used as fallback value if no option value is specified.
        /// </summary>
        public string? EnvironmentVariableName { get; set; }

        /// <summary>
        /// Optional sample value that will be used in help text for usage of required options. In case this property is not 
        /// set for a required option option's name will be used as sample value.
        /// </summary>
        public string? SampleValue { get; set; }

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