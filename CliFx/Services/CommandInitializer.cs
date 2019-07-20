using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    public class CommandInitializer : ICommandInitializer
    {
        private readonly ITypeActivator _typeActivator;
        private readonly ICommandSchemaResolver _commandSchemaResolver;
        private readonly ICommandOptionInputConverter _commandOptionInputConverter;

        public CommandInitializer(ITypeActivator typeActivator, ICommandSchemaResolver commandSchemaResolver,
            ICommandOptionInputConverter commandOptionInputConverter)
        {
            _typeActivator = typeActivator;
            _commandSchemaResolver = commandSchemaResolver;
            _commandOptionInputConverter = commandOptionInputConverter;
        }

        public CommandInitializer(ICommandSchemaResolver commandSchemaResolver)
            : this(new TypeActivator(), commandSchemaResolver, new CommandOptionInputConverter())
        {
        }

        public CommandInitializer()
            : this(new CommandSchemaResolver())
        {
        }

        private CommandSchema GetDefaultSchema(IReadOnlyList<CommandSchema> schemas)
        {
            // Get command types marked as default
            var defaultSchemas = schemas.Where(t => t.IsDefault).ToArray();

            // If there's only one type - return
            if (defaultSchemas.Length == 1)
                return defaultSchemas.Single();

            // If there are multiple - throw
            if (defaultSchemas.Length > 1)
            {
                throw new CommandResolveException(
                    "Can't resolve default command because there is more than one command marked as default. " +
                    $"Make sure you apply {nameof(CommandAttribute)} only to one command.");
            }

            // If there aren't any - throw
            throw new CommandResolveException(
                "Can't resolve default command because there are no commands marked as default. " +
                $"Apply {nameof(CommandAttribute)} to the default command.");
        }

        private CommandSchema GetSchemaByName(IReadOnlyList<CommandSchema> schemas, string name)
        {
            // Get command types with given name
            var matchingSchemas =
                schemas.Where(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)).ToArray();

            // If there's only one type - return
            if (matchingSchemas.Length == 1)
                return matchingSchemas.Single();

            // If there are multiple - throw
            if (matchingSchemas.Length > 1)
            {
                throw new CommandResolveException(
                    $"Can't resolve command because there is more than one command named [{name}]. " +
                    "Make sure all command names are unique and keep in mind that comparison is case-insensitive.");
            }

            // If there aren't any - throw
            throw new CommandResolveException(
                $"Can't resolve command because none of the commands is named [{name}]. " +
                $"Apply {nameof(CommandAttribute)} to give command a name.");
        }

        // TODO: refactor
        public ICommand InitializeCommand(CommandInput input)
        {
            var schemas = _commandSchemaResolver.ResolveAllSchemas();

            // Get command type
            var schema = !input.CommandName.IsNullOrWhiteSpace()
                ? GetSchemaByName(schemas, input.CommandName)
                : GetDefaultSchema(schemas);

            // Activate command
            var command = (ICommand) _typeActivator.Activate(schema.Type);
            command.Context = new CommandContext(schemas, schema);

            // Set command options
            var isGroupNameDetected = false;
            var groupName = default(string);
            var properties = new HashSet<CommandOptionSchema>();
            foreach (var option in input.Options)
            {
                var optionInfo = schema.Options.FirstOrDefault(p =>
                    string.Equals(p.Name, option.Name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.ShortName?.AsString(), option.Name, StringComparison.OrdinalIgnoreCase));

                if (optionInfo == null)
                    continue;

                if (isGroupNameDetected && !string.Equals(groupName, optionInfo.GroupName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!isGroupNameDetected)
                {
                    groupName = optionInfo.GroupName;
                    isGroupNameDetected = true;
                }

                var convertedValue = _commandOptionInputConverter.ConvertOption(option, optionInfo.Property.PropertyType);
                optionInfo.Property.SetValue(command, convertedValue);

                properties.Add(optionInfo);
            }

            var unsetRequiredOptions = schema.Options
                .Except(properties)
                .Where(p => p.IsRequired)
                .Where(p => string.Equals(p.GroupName, groupName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (unsetRequiredOptions.Any())
                throw new CommandResolveException(
                    $"Can't resolve command because one or more required properties were not set: {unsetRequiredOptions.Select(p => p.Name).JoinToString(", ")}");

            return command;
        }
    }
}