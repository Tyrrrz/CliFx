using System;
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

        public bool MatchesName(string name) => string.Equals(name, Name, StringComparison.OrdinalIgnoreCase);

        public bool MatchesShortName(char shortName) => shortName == ShortName;

        public bool MatchesEnvironmentVariableName(string environmentVariableName) =>
            string.Equals(environmentVariableName, EnvironmentVariableName, StringComparison.OrdinalIgnoreCase);

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
}