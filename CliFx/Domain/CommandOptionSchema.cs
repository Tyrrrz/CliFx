using System;
using System.Linq;
using System.Reflection;
using System.Text;
using CliFx.Attributes;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal partial class CommandOptionSchema : CommandArgumentSchema
    {
        public string? Name { get; }

        public char? ShortName { get; }

        public string DisplayName => !string.IsNullOrWhiteSpace(Name)
            ? Name
            : ShortName?.AsString()!;

        public string? EnvironmentVariableName { get; }

        public bool IsRequired { get; }

        public CommandOptionSchema(
            PropertyInfo property,
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

        public bool MatchesName(string name) =>
            !string.IsNullOrWhiteSpace(Name) &&
            string.Equals(Name, name, StringComparison.OrdinalIgnoreCase);

        public bool MatchesShortName(char shortName) =>
            ShortName != null &&
            ShortName == shortName;

        public bool MatchesNameOrShortName(string alias) =>
            MatchesName(alias) ||
            alias.Length == 1 && MatchesShortName(alias.Single());

        public bool MatchesEnvironmentVariableName(string environmentVariableName) =>
            !string.IsNullOrWhiteSpace(EnvironmentVariableName) &&
            string.Equals(EnvironmentVariableName, environmentVariableName, StringComparison.OrdinalIgnoreCase);

        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Name))
            {
                buffer.Append("--");
                buffer.Append(Name);
            }

            if (!string.IsNullOrWhiteSpace(Name) && ShortName != null)
                buffer.Append('|');

            if (ShortName != null)
            {
                buffer.Append('-');
                buffer.Append(ShortName);
            }

            return buffer.ToString();
        }
    }

    internal partial class CommandOptionSchema
    {
        public static CommandOptionSchema? TryResolve(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<CommandOptionAttribute>();
            if (attribute == null)
                return null;

            return new CommandOptionSchema(
                property,
                attribute.Name,
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
            new CommandOptionSchema(null!, "help", 'h', null, false, "Shows help text.");

        public static CommandOptionSchema VersionOption { get; } =
            new CommandOptionSchema(null!, "version", null, null, false, "Shows version information.");
    }
}