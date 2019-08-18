using System.Reflection;
using System.Text;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Schema of a defined command option.
    /// </summary>
    public partial class CommandOptionSchema
    {
        /// <summary>
        /// Underlying property.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Option name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Option short name.
        /// </summary>
        public char? ShortName { get; }

        /// <summary>
        /// Whether an option is required.
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// Option description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionSchema"/>.
        /// </summary>
        public CommandOptionSchema(PropertyInfo property, string name, char? shortName, bool isRequired, string description)
        {
            Property = property; // can be null
            Name = name; // can be null
            ShortName = shortName; // can be null
            IsRequired = isRequired;
            Description = description; // can be null
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (IsRequired)
                buffer.Append('*');

            if (!Name.IsNullOrWhiteSpace())
                buffer.Append(Name);

            if (!Name.IsNullOrWhiteSpace() && ShortName != null)
                buffer.Append('|');

            if (ShortName != null)
                buffer.Append(ShortName);

            return buffer.ToString();
        }
    }

    public partial class CommandOptionSchema
    {
        // Here we define some built-in options.
        // This is probably a bit hacky but I couldn't come up with a better solution given this architecture.
        // We define them here to serve as a single source of truth, because they are used...
        // ...in CliApplication (when reading) and HelpTextRenderer (when writing).

        internal static CommandOptionSchema Help { get; } =
            new CommandOptionSchema(null, "help", 'h', false, "Shows help text.");

        internal static CommandOptionSchema Version { get; } =
            new CommandOptionSchema(null, "version", null, false, "Shows version information.");
    }
}