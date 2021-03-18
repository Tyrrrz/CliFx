using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CliFx.Attributes;

namespace CliFx.Schema
{
    internal partial class OptionSchema : IMemberSchema
    {
        public IPropertyDescriptor Property { get; }

        public string? Name { get; }

        public char? ShortName { get; }

        public string? EnvironmentVariable { get; }

        public bool IsRequired { get; }

        public string? Description { get; }

        public Type? ConverterType { get; }

        public IReadOnlyList<Type> ValidatorTypes { get; }

        public OptionSchema(
            IPropertyDescriptor property,
            string? name,
            char? shortName,
            string? environmentVariable,
            bool isRequired,
            string? description,
            Type? converterType,
            IReadOnlyList<Type> validatorTypes)
        {
            Property = property;
            Name = name;
            ShortName = shortName;
            EnvironmentVariable = environmentVariable;
            IsRequired = isRequired;
            Description = description;
            ConverterType = converterType;
            ValidatorTypes = validatorTypes;
        }

        public bool MatchesName(string? name) =>
            !string.IsNullOrWhiteSpace(Name) &&
            string.Equals(Name, name, StringComparison.OrdinalIgnoreCase);

        public bool MatchesShortName(char? shortName) =>
            ShortName is not null &&
            ShortName == shortName;

        public bool MatchesIdentifier(string identifier) =>
            MatchesName(identifier) ||
            identifier.Length == 1 && MatchesShortName(identifier[0]);

        public bool MatchesEnvironmentVariable(string environmentVariableName) =>
            !string.IsNullOrWhiteSpace(EnvironmentVariable) &&
            string.Equals(EnvironmentVariable, environmentVariableName, StringComparison.Ordinal);

        public string GetFormattedIdentifier()
        {
            var buffer = new StringBuilder();

            // Short name
            if (ShortName is not null)
            {
                buffer
                    .Append('-')
                    .Append(ShortName);
            }

            // Separator
            if (!string.IsNullOrWhiteSpace(Name) && ShortName is not null)
            {
                buffer.Append('|');
            }

            // Name
            if (!string.IsNullOrWhiteSpace(Name))
            {
                buffer
                    .Append("--")
                    .Append(Name);
            }

            return buffer.ToString();
        }
    }

    internal partial class OptionSchema
    {
        public static OptionSchema? TryResolve(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<CommandOptionAttribute>();
            if (attribute is null)
                return null;

            // The user may mistakenly specify dashes, thinking it's required, so trim them
            var name = attribute.Name?.TrimStart('-');

            return new OptionSchema(
                new BindablePropertyDescriptor(property),
                name,
                attribute.ShortName,
                attribute.EnvironmentVariable,
                attribute.IsRequired,
                attribute.Description,
                attribute.Converter,
                attribute.Validators
            );
        }
    }

    internal partial class OptionSchema
    {
        public static OptionSchema HelpOption { get; } = new(
            NullPropertyDescriptor.Instance,
            "help",
            'h',
            null,
            false,
            "Shows help text.",
            null,
            Array.Empty<Type>()
        );

        public static OptionSchema VersionOption { get; } = new(
            NullPropertyDescriptor.Instance,
            "version",
            null,
            null,
            false,
            "Shows version information.",
            null,
            Array.Empty<Type>()
        );
    }
}