using System.Reflection;
using System.Text;
using CliFx.Internal;

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
}