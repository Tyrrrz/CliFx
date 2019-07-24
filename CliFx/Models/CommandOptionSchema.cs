using System.Reflection;

namespace CliFx.Models
{
    public class CommandOptionSchema
    {
        public PropertyInfo Property { get; }

        public string Name { get; }

        public char? ShortName { get; }

        public string GroupName { get; }

        public bool IsRequired { get; }

        public string Description { get; }

        public CommandOptionSchema(PropertyInfo property, string name, char? shortName,
            string groupName, bool isRequired, string description)
        {
            Property = property;
            Name = name;
            ShortName = shortName;
            IsRequired = isRequired;
            GroupName = groupName;
            Description = description;
        }
    }
}