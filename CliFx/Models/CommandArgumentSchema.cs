using System.Globalization;
using System.Reflection;
using System.Text;

namespace CliFx.Models
{
    /// <summary>
    /// Schema of a defined command argument.
    /// </summary>
    public class CommandArgumentSchema
    {
        /// <summary>
        /// Underlying property.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Argument name used for help text. If not supplied, the property name will be used.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Whether the argument is required.
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// Argument description.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Order of the argument.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The display name of the argument. Returns <see cref="Name"/> if specified, otherwise the name of the underlying property.
        /// </summary>
        public string DisplayName => !string.IsNullOrWhiteSpace(Name) ? Name! : Property.PropertyType.Name.ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// Initializes an instance of <see cref="CommandArgumentSchema"/>.
        /// </summary>
        public CommandArgumentSchema(PropertyInfo property, string? name, bool isRequired, string? description, int order)
        {
            Property = property;
            Name = name;
            IsRequired = isRequired;
            Description = description;
            Order = order;
        }

        /// <summary>
        /// Returns the string representation of the argument schema.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (!IsRequired)
            {
                sb.Append("[");
            }

            sb.Append("<");
            sb.Append($"{DisplayName}");
            sb.Append(">");

            if (!IsRequired)
            {
                sb.Append("]");
            }

            return sb.ToString();
        }
    }
}