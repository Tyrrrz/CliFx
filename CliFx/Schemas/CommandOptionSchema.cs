namespace CliFx.Schemas
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using CliFx.Attributes;

    /// <summary>
    /// Stores command option schema.
    /// </summary>
    public partial class CommandOptionSchema : CommandArgumentSchema
    {
        /// <summary>
        /// Option name.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Option short name.
        /// </summary>
        public char? ShortName { get; }

        /// <summary>
        /// Name of environment variable used as a fallback value.
        /// </summary>
        public string? EnvironmentVariableName { get; }

        /// <summary>
        /// Whether option is required.
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionSchema"/>.
        /// </summary>
        internal CommandOptionSchema(PropertyInfo? property,
                                     string? name,
                                     char? shortName,
                                     string? environmentVariableName,
                                     bool isRequired,
                                     string? description)
            : base(property, description)
        {
            Name = name;
            ShortName = shortName;
            EnvironmentVariableName = environmentVariableName;
            IsRequired = isRequired;
        }

        /// <summary>
        /// Whether command's name matches the passed name.
        /// </summary>
        public bool MatchesName(string name)
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   string.Equals(Name, name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Whether command's short name matches the passed short name.
        /// </summary>
        public bool MatchesShortName(char shortName)
        {
            return ShortName != null && ShortName == shortName;
        }

        /// <summary>
        /// Whether command's name or short name matches the passed name.
        /// </summary>
        public bool MatchesNameOrShortName(string alias)
        {
            return MatchesName(alias) ||
                   alias.Length == 1 && MatchesShortName(alias.Single());
        }

        /// <summary>
        /// Whether command's environment variable matches the passed environment variable name.
        /// </summary>
        public bool MatchesEnvironmentVariableName(string environmentVariableName)
        {
            return !string.IsNullOrWhiteSpace(EnvironmentVariableName) &&
                   string.Equals(EnvironmentVariableName, environmentVariableName, StringComparison.Ordinal);
        }

        internal string GetUserFacingDisplayString()
        {
            var buffer = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Name))
            {
                buffer.Append("--")
                      .Append(Name);
            }

            if (!string.IsNullOrWhiteSpace(Name) && ShortName != null)
            {
                buffer.Append('|');
            }

            if (ShortName != null)
            {
                buffer.Append('-')
                      .Append(ShortName);
            }

            return buffer.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Property?.Name ?? "<implicit>"} ('{GetUserFacingDisplayString()}')";
        }
    }

    public partial class CommandOptionSchema
    {
        internal static CommandOptionSchema? TryResolve(PropertyInfo property)
        {
            CommandOptionAttribute? attribute = property.GetCustomAttribute<CommandOptionAttribute>();
            if (attribute is null)
                return null;

            // The user may mistakenly specify dashes, thinking it's required, so trim them
            string name = attribute.Name?.TrimStart('-');

            return new CommandOptionSchema(
                property,
                name,
                attribute.ShortName,
                attribute.EnvironmentVariableName,
                attribute.IsRequired,
                attribute.Description
            );
        }
    }

    public partial class CommandOptionSchema
    {
        internal static CommandOptionSchema HelpOption { get; } =
            new CommandOptionSchema(null, "help", 'h', null, false, "Shows help text.");

        internal static CommandOptionSchema VersionOption { get; } =
            new CommandOptionSchema(null, "version", null, null, false, "Shows version information.");
    }
}