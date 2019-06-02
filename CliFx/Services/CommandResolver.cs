using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Internal;

namespace CliFx.Services
{
    public class CommandResolver : ICommandResolver
    {
        private readonly ITypeProvider _typeProvider;
        private readonly ICommandOptionParser _commandOptionParser;
        private readonly ICommandOptionConverter _commandOptionConverter;

        public CommandResolver(ITypeProvider typeProvider,
            ICommandOptionParser commandOptionParser, ICommandOptionConverter commandOptionConverter)
        {
            _typeProvider = typeProvider;
            _commandOptionParser = commandOptionParser;
            _commandOptionConverter = commandOptionConverter;
        }

        private IEnumerable<CommandType> GetCommandTypes() => CommandType.GetCommandTypes(_typeProvider.GetTypes());

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

        public Command ResolveCommand(IReadOnlyList<string> commandLineArguments)
        {
            var optionSet = _commandOptionParser.ParseOptions(commandLineArguments);

            // Get command type
            var commandType = !optionSet.CommandName.IsNullOrWhiteSpace()
                ? GetCommandType(optionSet.CommandName)
                : GetDefaultCommandType();

            // Activate command
            var command = commandType.Activate();

            // Set command options
            foreach (var property in commandType.GetOptionProperties())
            {
                // If option set contains this property - set value
                if (optionSet.Options.TryGetValue(property.Name, out var value) ||
                    optionSet.Options.TryGetValue(property.ShortName.ToString(CultureInfo.InvariantCulture), out value))
                {
                    var convertedValue = _commandOptionConverter.ConvertOption(value, property.Type);
                    property.SetValue(command, convertedValue);
                }
                // If the property is missing but it's required - throw
                else if (property.IsRequired)
                {
                    throw new CommandResolveException(
                        $"Can't resolve command [{optionSet.CommandName}] because required property [{property.Name}] is not set.");
                }
            }

            return command;
        }
    }
}