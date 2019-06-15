using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;

namespace CliFx.Internal
{
    internal partial class CommandType
    {
        private readonly Type _type;

        public string Name { get; }

        public bool IsDefault { get; }

        public string Description { get; }

        public IReadOnlyList<CommandOptionProperty> Options { get; }

        public CommandType(Type type, string name, bool isDefault, string description, IReadOnlyList<CommandOptionProperty> options)
        {
            _type = type;
            Name = name;
            IsDefault = isDefault;
            Description = description;
            Options = options;
        }

        public Command Activate() => (Command) Activator.CreateInstance(_type);
    }

    internal partial class CommandType
    {
        public static bool IsValid(Type type) =>
            // Derives from Command
            type.IsDerivedFrom(typeof(Command)) &&
            // Marked with DefaultCommandAttribute or CommandAttribute
            (type.IsDefined(typeof(DefaultCommandAttribute)) || type.IsDefined(typeof(CommandAttribute)));

        public static CommandType Initialize(Type type)
        {
            if (!IsValid(type))
                throw new InvalidOperationException($"[{type.Name}] is not a valid command type.");

            var attribute = type.GetCustomAttribute<CommandAttribute>();

            var name = attribute.Name;
            var isDefault = attribute is DefaultCommandAttribute;
            var description = attribute.Description;

            var options = type.GetProperties()
                .Where(CommandOptionProperty.IsValid)
                .Select(CommandOptionProperty.Initialize)
                .ToArray();

            return new CommandType(type, name, isDefault, description, options);
        }

        public static IEnumerable<CommandType> GetCommandTypes(IEnumerable<Type> types) => types.Where(IsValid).Select(Initialize);
    }
}