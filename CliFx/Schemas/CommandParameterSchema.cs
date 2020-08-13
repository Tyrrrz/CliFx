namespace CliFx.Schemas
{
    using System.Reflection;
    using System.Text;
    using CliFx.Attributes;

    /// <summary>
    /// Stores command parameter schema.
    /// </summary>
    public partial class CommandParameterSchema : CommandArgumentSchema
    {
        /// <summary>
        /// Parameter order.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Parameter name.
        /// </summary>
        public string Name { get; }

        internal CommandParameterSchema(PropertyInfo? property, int order, string name, string? description)
            : base(property, description)
        {
            Order = order;
            Name = name;
        }

        internal string GetUserFacingDisplayString()
        {
            var buffer = new StringBuilder();

            buffer.Append('<')
                  .Append(Name)
                  .Append('>');

            return buffer.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Property?.Name ?? "<implicit>"} ([{Order}] {GetUserFacingDisplayString()})";
        }
    }

    public partial class CommandParameterSchema
    {
        internal static CommandParameterSchema? TryResolve(PropertyInfo property)
        {
            CommandParameterAttribute? attribute = property.GetCustomAttribute<CommandParameterAttribute>();
            if (attribute is null)
                return null;

            string name = attribute.Name ?? property.Name.ToLowerInvariant();

            return new CommandParameterSchema(
                property,
                attribute.Order,
                name,
                attribute.Description
            );
        }
    }
}