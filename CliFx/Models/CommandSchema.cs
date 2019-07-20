using System;
using System.Collections.Generic;

namespace CliFx.Models
{
    public class CommandSchema
    {
        public Type Type { get; }

        public string Name { get; }

        public bool IsDefault { get; }

        public string Description { get; }

        public IReadOnlyList<CommandOptionSchema> Options { get; }

        public CommandSchema(Type type, string name, bool isDefault, string description, IReadOnlyList<CommandOptionSchema> options)
        {
            Type = type;
            Name = name;
            IsDefault = isDefault;
            Description = description;
            Options = options;
        }
    }
}