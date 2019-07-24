using System;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    public class CommandSchemaResolver : ICommandSchemaResolver
    {
        private CommandOptionSchema GetCommandOptionSchema(PropertyInfo optionProperty)
        {
            var attribute = optionProperty.GetCustomAttribute<CommandOptionAttribute>();

            if (attribute == null)
                return null;

            return new CommandOptionSchema(optionProperty,
                attribute.Name,
                attribute.ShortName,
                attribute.GroupName,
                attribute.IsRequired, attribute.Description);
        }

        // TODO: validate stuff like duplicate names, multiple default commands, etc
        public CommandSchema GetCommandSchema(Type commandType)
        {
            if (!commandType.Implements(typeof(ICommand)))
                throw new ArgumentException($"Command type must implement {nameof(ICommand)}.", nameof(commandType));

            var attribute = commandType.GetCustomAttribute<CommandAttribute>();

            var options = commandType.GetProperties().Select(GetCommandOptionSchema).ExceptNull().ToArray();

            return new CommandSchema(commandType,
                attribute?.Name,
                attribute?.Description,
                options);
        }
    }
}