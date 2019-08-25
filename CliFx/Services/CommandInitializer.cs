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
        public void InitializeCommand(ICommand command, CommandSchema commandSchema, CommandInput commandInput)
        {
            command.GuardNotNull(nameof(command));
            commandSchema.GuardNotNull(nameof(commandSchema));
            commandInput.GuardNotNull(nameof(commandInput));

            // Keep track of unset required options to report an error at a later stage
            var unsetRequiredOptions = commandSchema.Options.Where(o => o.IsRequired).ToList();

            // Set command options
            foreach (var optionInput in commandInput.Options)
            {
                // Find matching option schema for this option input
                var optionSchema = commandSchema.Options.FindByAlias(optionInput.Alias);
                if (optionSchema == null)
                    continue;

                // Convert option to the type of the underlying property
                var convertedValue = _commandOptionInputConverter.ConvertOptionInput(optionInput, optionSchema.Property.PropertyType);

                // Set value of the underlying property
                optionSchema.Property.SetValue(command, convertedValue);

                // Mark this required option as set
                if (optionSchema.IsRequired)
                    unsetRequiredOptions.Remove(optionSchema);
            }

            // Throw if any of the required options were not set
            if (unsetRequiredOptions.Any())
            {
                var unsetRequiredOptionNames = unsetRequiredOptions.Select(o => o.GetAliases().FirstOrDefault()).JoinToString(", ");
                throw new CliFxException($"One or more required options were not set: {unsetRequiredOptionNames}.");
            }
        }
    }
}