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

            // If an attribute is not set, then it's not an option so we just skip it
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
            var attribute = commandType.GetCustomAttribute<CommandAttribute>();

            // Make sure attribute is set
            if (attribute == null)
            {
                throw new InvalidCommandSchemaException($"Command type [{commandType}] must be annotated with [{typeof(CommandAttribute)}].");
            }

            // Get option schemas
            var options = commandType.GetProperties().Select(GetCommandOptionSchema).ExceptNull().ToArray();

            // Create command schema
            var commandSchema = new CommandSchema(commandType,
                attribute.Name,
                attribute.Description,
                options);

            // Make sure command type implements ICommand.
            // (we check using command schema to provide a more useful error message)
            if (!commandSchema.Type.Implements(typeof(ICommand)))
            {
                throw new InvalidCommandSchemaException(!commandSchema.Name.IsNullOrWhiteSpace()
                    ? $"Command [{commandSchema.Name}] doesn't implement ICommand."
                    : "Default command doesn't implement ICommand.");
            }

            // Make sure there are no options with duplicate names
            var nonUniqueOptionName = options
                .Where(o => !o.Name.IsNullOrWhiteSpace())
                .Select(o => o.Name)
                .GroupBy(i => i, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() >= 2)
                .SelectMany(g => g)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (nonUniqueOptionName != null)
            {
                throw new InvalidCommandSchemaException(!commandSchema.Name.IsNullOrWhiteSpace()
                    ? $"There are multiple options defined with name [{nonUniqueOptionName}] on command [{commandSchema.Name}]."
                    : $"There are multiple options defined with name [{nonUniqueOptionName}] on default command.");
            }

            // Make sure there are no options with duplicate short names
            var nonUniqueOptionShortName = commandSchema.Options
                .Where(o => o.ShortName != null)
                .Select(o => o.ShortName)
                .GroupBy(i => i)
                .Where(g => g.Count() >= 2)
                .SelectMany(g => g)
                .Distinct()
                .FirstOrDefault();

            if (nonUniqueOptionShortName != null)
            {
                throw new InvalidCommandSchemaException(!commandSchema.Name.IsNullOrWhiteSpace()
                    ? $"There are multiple options defined with short name [{nonUniqueOptionShortName}] on command [{commandSchema.Name}]."
                    : $"There are multiple options defined with short name [{nonUniqueOptionShortName}] on default command.");
            }

            return commandSchema;
        }

        /// <inheritdoc />
        public IReadOnlyList<CommandSchema> GetCommandSchemas(IReadOnlyList<Type> commandTypes)
        {
            commandTypes.GuardNotNull(nameof(commandTypes));

            // Throw if there are no command types specified
            if (!commandTypes.Any())
            {
                throw new InvalidCommandSchemaException("There are no commands defined.");
            }

            // Get command schemas
            var commandSchemas = commandTypes.Select(GetCommandSchema).ToArray();

            // Make sure there are no commands with duplicate names
            var nonUniqueCommandName = commandSchemas
                .Select(c => c.Name)
                .GroupBy(i => i, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() >= 2)
                .SelectMany(g => g)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (nonUniqueCommandName != null)
            {
                throw new InvalidCommandSchemaException(!nonUniqueCommandName.IsNullOrWhiteSpace()
                    ? $"There are multiple commands defined with name [{nonUniqueCommandName}]."
                    : "There are multiple default commands defined.");
            }

            return commandSchemas;
        }
    }
}