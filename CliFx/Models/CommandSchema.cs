using System;
using System.Collections.Generic;
using System.Text;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Schema of a defined command.
    /// </summary>
    public class CommandSchema
    {
        /// <summary>
        /// Underlying type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Command name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Command description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Command options.
        /// </summary>
        public IReadOnlyList<CommandOptionSchema> Options { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandSchema"/>.
        /// </summary>
        public CommandSchema(Type type, string name, string description, IReadOnlyList<CommandOptionSchema> options)
        {
            Type = type; // can be null
            Name = name; // can be null
            Description = description; // can be null
            Options = options.GuardNotNull(nameof(options));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (!Name.IsNullOrWhiteSpace())
            {
                buffer.Append(Name);
                buffer.Append(' ');
            }

            foreach (var option in Options)
            {
                buffer.Append('[');
                buffer.Append(option);
                buffer.Append(']');
                buffer.Append(' ');
            }

            return buffer.Trim().ToString();
        }
    }
}