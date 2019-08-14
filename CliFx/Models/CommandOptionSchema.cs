using System.Reflection;
using System.Text;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Schema of a defined command option.
    /// </summary>
    public class CommandOptionSchema
    {
        /// <summary>
        /// Underlying property.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Option name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Option short name.
        /// </summary>
        public char? ShortName { get; }

        /// <summary>
        /// Option group name.
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Whether an option is required.
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// Option description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionSchema"/>.
        /// </summary>
        public CommandOptionSchema(PropertyInfo property, string name, char? shortName,
            string groupName, bool isRequired, string description)
        {
            Property = property; // can be null
            Name = name; // can be null
            ShortName = shortName; // can be null
            IsRequired = isRequired;
            GroupName = groupName; // can be null
            Description = description; // can be null
        }

        /// <inheritdoc />
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