using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Exceptions;
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
                attribute.IsRequired,
                attribute.Description);
        }

        private CommandSchema GetCommandSchema(Type commandType)
        {
            // Attribute is optional for commands in order to reduce runtime rule complexity
            var attribute = commandType.GetCustomAttribute<CommandAttribute>();

            var options = commandType.GetProperties().Select(GetCommandOptionSchema).ExceptNull().ToArray();

            return new CommandSchema(commandType,
                attribute?.Name,
                attribute?.Description,
                options);
        }

        /// <inheritdoc />
        public IReadOnlyList<CommandSchema> GetCommandSchemas(IReadOnlyList<Type> commandTypes)
        {
            commandTypes.GuardNotNull(nameof(commandTypes));

            // Get command schemas
            var commandSchemas = commandTypes.Select(GetCommandSchema).ToArray();

            // Throw if there are no commands defined
            if (!commandSchemas.Any())
            {
                throw new InvalidCommandSchemaException("There are no commands defined.");
            }

            // Throw if there are multiple commands with the same name
            var nonUniqueCommandNames = commandSchemas
                .Select(c => c.Name)
                .GroupBy(i => i, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() >= 2)
                .SelectMany(g => g)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var commandName in nonUniqueCommandNames)
            {
                throw new InvalidCommandSchemaException(!commandName.IsNullOrWhiteSpace()
                    ? $"There are multiple commands defined with name [{commandName}]."
                    : "There are multiple default commands defined.");
            }

            // Throw if there are commands that don't implement ICommand
            var nonImplementedCommandNames = commandSchemas
                .Where(c => !c.Type.Implements(typeof(ICommand)))
                .Select(c => c.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var commandName in nonImplementedCommandNames)
            {
                throw new InvalidCommandSchemaException(!commandName.IsNullOrWhiteSpace()
                    ? $"Command [{commandName}] doesn't implement ICommand."
                    : "Default command doesn't implement ICommand.");
            }

            // Throw if there are multiple options with the same name inside the same command
            foreach (var commandSchema in commandSchemas)
            {
                var nonUniqueOptionNames = commandSchema.Options
                    .Where(o => !o.Name.IsNullOrWhiteSpace())
                    .Select(o => o.Name)
                    .GroupBy(i => i, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() >= 2)
                    .SelectMany(g => g)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                foreach (var optionName in nonUniqueOptionNames)
                {
                    throw new InvalidCommandSchemaException(!commandSchema.Name.IsNullOrWhiteSpace()
                        ? $"There are multiple options defined with name [{optionName}] on command [{commandSchema.Name}]."
                        : $"There are multiple options defined with name [{optionName}] on default command.");
                }

                var nonUniqueOptionShortNames = commandSchema.Options
                    .Where(o => o.ShortName != null)
                    .Select(o => o.ShortName.Value)
                    .GroupBy(i => i)
                    .Where(g => g.Count() >= 2)
                    .SelectMany(g => g)
                    .Distinct()
                    .ToArray();

                foreach (var optionShortName in nonUniqueOptionShortNames)
                {
                    throw new InvalidCommandSchemaException(!commandSchema.Name.IsNullOrWhiteSpace()
                        ? $"There are multiple options defined with short name [{optionShortName}] on command [{commandSchema.Name}]."
                        : $"There are multiple options defined with short name [{optionShortName}] on default command.");
                }
            }

            return commandSchemas;
        }
    }
}