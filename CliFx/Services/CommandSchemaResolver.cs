using System;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandSchemaResolver"/>.
    /// </summary>
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
                attribute.IsRequired,
                attribute.Description);
        }

        /// <inheritdoc />
        public CommandSchema GetCommandSchema(Type commandType)
        {
            commandType.GuardNotNull(nameof(commandType));

            var attribute = commandType.GetCustomAttribute<CommandAttribute>();

            var options = commandType.GetProperties().Select(GetCommandOptionSchema).ExceptNull().ToArray();

            return new CommandSchema(commandType,
                attribute?.Name,
                attribute?.Description,
                options);
        }
    }
}