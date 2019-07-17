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
    public partial class CommandResolver : ICommandResolver
    {
        private readonly IReadOnlyList<Type> _availableTypes;
        private readonly ICommandOptionConverter _commandOptionConverter;

        public CommandResolver(IReadOnlyList<Type> availableTypes, ICommandOptionConverter commandOptionConverter)
        {
            _availableTypes = availableTypes;
            _commandOptionConverter = commandOptionConverter;
        }

        public CommandResolver(ICommandOptionConverter commandOptionConverter)
            : this(GetDefaultAvailableTypes(), commandOptionConverter)
        {
        }

        private IEnumerable<CommandType> GetCommandTypes() => CommandType.GetCommandTypes(_availableTypes);

        private CommandType GetDefaultCommandType()
        {
            // Get command types marked as default
            var defaultCommandTypes = GetCommandTypes().Where(t => t.IsDefault).ToArray();

            // If there's only one type - return
            if (defaultCommandTypes.Length == 1)
                return defaultCommandTypes.Single();

            // If there are multiple - throw
            if (defaultCommandTypes.Length > 1)
            {
                throw new CommandResolveException(
                    "Can't resolve default command because there is more than one command marked as default. " +
                    $"Make sure you apply {nameof(DefaultCommandAttribute)} only to one command.");
            }

            // If there aren't any - throw
            throw new CommandResolveException(
                "Can't resolve default command because there are no commands marked as default. " +
                $"Apply {nameof(DefaultCommandAttribute)} to the default command.");
        }

        private CommandType GetCommandType(string name)
        {
            // Get command types with given name
            var matchingCommandTypes =
                GetCommandTypes().Where(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)).ToArray();

            // If there's only one type - return
            if (matchingCommandTypes.Length == 1)
                return matchingCommandTypes.Single();

            // If there are multiple - throw
            if (matchingCommandTypes.Length > 1)
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

        public Command ResolveCommand(CommandOptionSet optionSet)
        {
            // Get command type
            var commandType = !optionSet.CommandName.IsNullOrWhiteSpace()
                ? GetCommandType(optionSet.CommandName)
                : GetDefaultCommandType();

            // Activate command
            var command = commandType.Activate();

            // Set command options
            foreach (var property in commandType.Options)
            {
                // Get option for this property
                var option = optionSet.GetOptionOrDefault(property.Name, property.ShortName);

                // If there are any matching options - set value
                if (option != null)
                {
                    var convertedValue = _commandOptionConverter.ConvertOption(option, property.Type);
                    property.SetValue(command, convertedValue);
                }
                // If the property is required but it's missing - throw
                else if (property.IsRequired)
                {
                    throw new CommandResolveException($"Can't resolve command because required property [{property.Name}] is not set.");
                }
            }

            return command;
        }
    }

    public partial class CommandResolver
    {
        private static IReadOnlyList<Type> GetDefaultAvailableTypes() => Assembly.GetEntryAssembly()?.GetExportedTypes() ?? new Type[0];
    }
}