using System.Collections.Generic;
using System.Reflection;
using CliFx.Internal;

namespace CliFx.Models
{
    public class CommandOptionSchema
    {
        public PropertyInfo Property { get; }

        public string Name { get; }

        public char? ShortName { get; }

        public bool IsRequired { get; }

        public string GroupName { get; }

        public string Description { get; }

        public CommandOptionSchema(PropertyInfo property, string name, char? shortName,
            bool isRequired, string groupName, string description)
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