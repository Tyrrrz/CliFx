using System;
using System.Collections.Generic;
using System.Text;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <inheritdoc />
    public partial class CommandSchema : ICommandSchema
    {
        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public string? Name { get; }

        /// <inheritdoc />
        public string? Description { get; }

        /// <inheritdoc />
        public IReadOnlyList<CommandOptionSchema> Options { get; }

        /// <inheritdoc />
        public IReadOnlyList<CommandArgumentSchema> Arguments { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandSchema"/>.
        /// </summary>
        public CommandSchema(Type type, string? name, string? description, IReadOnlyList<CommandArgumentSchema> arguments, IReadOnlyList<CommandOptionSchema> options)
        {
            Type = type;
            Name = name;
            Description = description;
            Options = options;
            Arguments = arguments;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Name))
                buffer.Append(Name);

            foreach (var option in Options)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer.Append('[');
                buffer.Append(option);
                buffer.Append(']');
            }

            return buffer.ToString();
        }
    }
}