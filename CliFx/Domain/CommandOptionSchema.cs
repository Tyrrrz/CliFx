using System;
using System.Linq;
using System.Reflection;
using System.Text;
using CliFx.Attributes;

namespace CliFx.Domain
{
    internal partial class CommandOptionSchema : CommandArgumentSchema
    {
        public string? Name { get; }

        public char? ShortName { get; }

        public string? EnvironmentVariableName { get; }

        public bool IsRequired { get; }

        public CommandOptionSchema(
            PropertyInfo? property,
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

        public bool MatchesName(string? name) =>
            !string.IsNullOrWhiteSpace(Name) &&
            string.Equals(Name, name, StringComparison.OrdinalIgnoreCase);

        public bool MatchesShortName(char? shortName) =>
            ShortName != null &&
            ShortName == shortName;

        public bool MatchesNameOrShortName(string alias) =>
            MatchesName(alias) ||
            alias.Length == 1 && MatchesShortName(alias.Single());

        public bool MatchesEnvironmentVariableName(string environmentVariableName) =>
            !string.IsNullOrWhiteSpace(EnvironmentVariableName) &&
            string.Equals(EnvironmentVariableName, environmentVariableName, StringComparison.Ordinal);

        public string GetUserFacingDisplayString()
        {
            var buffer = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Name))
            {
                buffer
                    .Append("--")
                    .Append(Name);
            }

            if (!string.IsNullOrWhiteSpace(Name) && ShortName != null)
            {
                buffer.Append('|');
            }

            if (ShortName != null)
            {
                buffer
                    .Append('-')
                    .Append(ShortName);
            }

            return buffer.ToString();
        }

        public string GetInternalDisplayString() => $"{Property?.Name ?? "<implicit>"} ('{GetUserFacingDisplayString()}')";

        public override string ToString() => GetInternalDisplayString();
    }

    internal partial class CommandOptionSchema
    {
        public static CommandOptionSchema? TryResolve(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<CommandOptionAttribute>();
            if (attribute == null)
                return null;

            // The user may mistakenly specify dashes, thinking it's required, so trim them
            var name = attribute.Name?.TrimStart('-');

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

    internal partial class CommandOptionSchema
    {
        public static CommandOptionSchema HelpOption { get; } =
            new CommandOptionSchema(null, "help", 'h', null, false, "Shows help text.");

        public static CommandOptionSchema VersionOption { get; } =
            new CommandOptionSchema(null, "version", null, null, false, "Shows version information.");
    }
}