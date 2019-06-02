using System;
using System.Reflection;
using CliFx.Attributes;

namespace CliFx.Internal
{
    internal partial class CommandOptionProperty
    {
        private readonly PropertyInfo _property;

        public Type Type => _property.PropertyType;

        public string Name { get; }

        public char ShortName { get; }

        public bool IsRequired { get; }

        public string Description { get; }

        public CommandOptionProperty(PropertyInfo property, string name, char shortName, bool isRequired, string description)
        {
            _property = property;
            Name = name;
            ShortName = shortName;
            IsRequired = isRequired;
            Description = description;
        }

        public void SetValue(Command command, object value) => _property.SetValue(command, value);
    }

    internal partial class CommandOptionProperty
    {
        public static bool IsValid(PropertyInfo property) => property.IsDefined(typeof(CommandOptionAttribute));

        public static CommandOptionProperty Initialize(PropertyInfo property)
        {
            if (!IsValid(property))
                throw new InvalidOperationException($"[{property.Name}] is not a valid command option property.");

            var attribute = property.GetCustomAttribute<CommandOptionAttribute>();

            return new CommandOptionProperty(property, attribute.Name, attribute.ShortName, attribute.IsRequired,
                attribute.Description);
        }
    }
}