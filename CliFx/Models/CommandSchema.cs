using System;
using System.Collections.Generic;

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
    }
}