namespace CliFx.Schemas
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using CliFx.Attributes;
    using CliFx.Exceptions;
    using CliFx.Internal.Extensions;

    /// <summary>
    /// Stores directive schema.
    /// </summary>
    public partial class DirectiveSchema
    {
        /// <summary>
        /// Directive type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Directive name.
        /// All directives in an application must have different names.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Directive description, which is used in help text.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Whether directive can run only in interactive mode.
        /// </summary>
        public bool InteractiveModeOnly { get; }

        private DirectiveSchema(Type type,
                                string name,
                                string? description,
                                bool interactiveModeOnly)
        {
            Type = type;
            Name = name;
            Description = description;
            InteractiveModeOnly = interactiveModeOnly;
        }

        internal string GetInternalDisplayString()
        {
            var buffer = new StringBuilder();

            // Type
            buffer.Append(Type.FullName);

            // Name
            buffer.Append(' ')
                  .Append('[')
                  .Append(Name)
                  .Append(']');

            return buffer.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return GetInternalDisplayString();
        }
    }

    public partial class DirectiveSchema
    {
        internal static bool IsDirectiveType(Type type)
        {
            return type.Implements(typeof(IDirective)) &&
                   type.IsDefined(typeof(DirectiveAttribute)) &&
                   !type.IsAbstract &&
                   !type.IsInterface;
        }

        internal static DirectiveSchema? TryResolve(Type type)
        {
            if (!IsDirectiveType(type))
                return null;

            DirectiveAttribute attribute = type.GetCustomAttribute<DirectiveAttribute>()!;

            var parameters = type.GetProperties()
                .Select(CommandParameterSchema.TryResolve)
                .Where(p => p != null)
                .ToArray();

            string name = attribute.Name.TrimStart('[').TrimEnd(']');
            if (string.IsNullOrWhiteSpace(name))
                throw CliFxException.DirectiveNameIsInvalid(name, type);

            return new DirectiveSchema(
                type,
                name,
                attribute.Description,
                attribute.InteractiveModeOnly
            );
        }
    }
}