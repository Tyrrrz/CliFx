using System.Linq;
using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandInitializer"/>.
    /// </summary>
    public class CommandInitializer : ICommandInitializer
    {
        private readonly ICommandOptionInputConverter _commandOptionInputConverter;

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer(ICommandOptionInputConverter commandOptionInputConverter)
        {
            _commandOptionInputConverter = commandOptionInputConverter.GuardNotNull(nameof(commandOptionInputConverter));
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer()
            : this(new CommandOptionInputConverter())
        {
        }

        /// <inheritdoc />
        public void InitializeCommand(ICommand command, CommandSchema schema, CommandInput input)
        {
            command.GuardNotNull(nameof(command));
            schema.GuardNotNull(nameof(schema));
            input.GuardNotNull(nameof(input));

            var unsetRequiredOptions = schema.Options.Where(o => o.IsRequired).ToList();

            // Set command options
            foreach (var option in input.Options)
            {
                var optionSchema = schema.Options.FindByAlias(option.Alias);

                if (optionSchema == null)
                    continue;

                var convertedValue = _commandOptionInputConverter.ConvertOption(option, optionSchema.Property.PropertyType);
                optionSchema.Property.SetValue(command, convertedValue);

                if (optionSchema.IsRequired)
                    unsetRequiredOptions.Remove(optionSchema);
            }

            // Throw if any of the required options were not set
            if (unsetRequiredOptions.Any())
            {
                var unsetRequiredOptionNames = unsetRequiredOptions.Select(o => o.GetAliases().FirstOrDefault()).JoinToString(", ");
                throw new MissingCommandOptionInputException($"One or more required options were not set: {unsetRequiredOptionNames}.");
            }
        }
    }
}