using System;
using System.Collections.Generic;
using System.Text;
using CliFx.Internal;

namespace CliFx.Models
{
    public class CommandSchema
    {
        public Type Type { get; }

        public string Name { get; }

        public string Description { get; }

        public IReadOnlyList<CommandOptionSchema> Options { get; }

        public CommandSchema(Type type, string name, string description, IReadOnlyList<CommandOptionSchema> options)
        {
            Type = type;
            Name = name;
            Description = description;
            Options = options;
        }

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