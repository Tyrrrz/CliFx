using System.Reflection;
using System.Text;

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
        public PropertyInfo? Property { get; }

        /// <summary>
        /// Option name.
        /// </summary>
        public string? Name { get; }

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
        public string? Description { get; }

        /// <summary>
        /// Optional environment variable name that will be used as fallback value if no option value is specified.
        /// </summary>
        public string? EnvironmentVariableName { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionSchema"/>.
        /// </summary>
        public CommandOptionSchema(PropertyInfo? property, string? name, char? shortName, bool isRequired, string? description, string? environmentVariableName)
        {
            Property = property;
            Name = name;
            ShortName = shortName;
            IsRequired = isRequired;
            Description = description;
            EnvironmentVariableName = environmentVariableName;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (IsRequired)
                buffer.Append('*');

            if (!string.IsNullOrWhiteSpace(Name))
                buffer.Append(Name);

            if (!string.IsNullOrWhiteSpace(Name) && ShortName != null)
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

        internal static CommandOptionSchema HelpOption { get; } =
            new CommandOptionSchema(null, "help", 'h', false, "Shows help text.", null);

        internal static CommandOptionSchema VersionOption { get; } =
            new CommandOptionSchema(null, "version", null, false, "Shows version information.", null);
    }
}