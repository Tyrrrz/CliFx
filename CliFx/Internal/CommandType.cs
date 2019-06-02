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

        public CommandType(Type type, string name, bool isDefault)
        {
            _type = type;
            Name = name;
            IsDefault = isDefault;
        }

        public IEnumerable<CommandOptionProperty> GetOptionProperties() => _type.GetProperties()
            .Where(CommandOptionProperty.IsValid)
            .Select(CommandOptionProperty.Initialize);

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

            var name = type.GetCustomAttribute<CommandAttribute>()?.Name;
            var isDefault = type.IsDefined(typeof(DefaultCommandAttribute));

            return new CommandType(type, name, isDefault);
        }

        public static IEnumerable<CommandType> GetCommandTypes(IEnumerable<Type> types) => types.Where(IsValid).Select(Initialize);
    }
}