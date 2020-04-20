using System.Reflection;
using System.Text;
using CliFx.Attributes;

namespace CliFx.Domain
{
    internal partial class CommandParameterSchema : CommandArgumentSchema
    {
        public int Order { get; }

        public string? Name { get; }

        public override string DisplayName =>
            !string.IsNullOrWhiteSpace(Name)
                ? Name
                : Property.Name.ToLowerInvariant();

        public CommandParameterSchema(PropertyInfo property, int order, string? name, string? description)
            : base(property, description)
        {
            Order = order;
            Name = name;
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer
                .Append('<')
                .Append(DisplayName)
                .Append('>');

            return buffer.ToString();
        }
    }

    internal partial class CommandParameterSchema
    {
        public static CommandParameterSchema? TryResolve(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<CommandParameterAttribute>();
            if (attribute == null)
                return null;

            return new CommandParameterSchema(
                property,
                attribute.Order,
                attribute.Name,
                attribute.Description
            );
        }
    }
}