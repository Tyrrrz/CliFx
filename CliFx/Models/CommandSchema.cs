using System;
using System.Collections.Generic;
using System.Text;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Schema of a defined command.
    /// </summary>
    public partial class CommandSchema
    {
        /// <summary>
        /// Underlying type.
        /// </summary>
        public Type? Type { get; }

        /// <summary>
        /// Command name.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Command description.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Command options.
        /// </summary>
        public IReadOnlyList<CommandOptionSchema> Options { get; }

        /// <summary>
        /// Command arguments.
        /// </summary>
        public IReadOnlyList<CommandArgumentSchema> Arguments { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandSchema"/>.
        /// </summary>
        public CommandSchema(Type type, string name, string description, IReadOnlyList<CommandArgumentSchema> arguments, IReadOnlyList<CommandOptionSchema> options)
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

            if (Options != null)
            {
                foreach (var option in Options)
                {
                    buffer.AppendIfNotEmpty(' ');
                    buffer.Append('[');
                    buffer.Append(option);
                    buffer.Append(']');
                }
            }
            return buffer.ToString();
        }
    }

    public partial class CommandSchema
    {
        internal static CommandSchema StubDefaultCommand { get; } =
            new CommandSchema(null, null, null, new CommandArgumentSchema[0], new CommandOptionSchema[0]);
    }
}