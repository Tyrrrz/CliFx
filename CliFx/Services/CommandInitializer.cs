using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;
using System.Linq;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandInitializer"/>.
    /// </summary>
    public class CommandInitializer : ICommandInitializer
    {
        private readonly ICommandOptionInputConverter _commandOptionInputConverter;
        private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer(ICommandOptionInputConverter commandOptionInputConverter, IEnvironmentVariablesProvider environmentVariablesProvider)
        {
            _commandOptionInputConverter = commandOptionInputConverter.GuardNotNull(nameof(commandOptionInputConverter));
            _environmentVariablesProvider = environmentVariablesProvider.GuardNotNull(nameof(environmentVariablesProvider));
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer(IEnvironmentVariablesProvider environmentVariablesProvider)
            : this(new CommandOptionInputConverter(), environmentVariablesProvider)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer()
            : this(new CommandOptionInputConverter(), new EnvironmentVariablesProvider())
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

            //Set command options
            foreach (var optionSchema in commandSchema.Options)
            {
                //Find matching option input
                CommandOptionInput optionInput = commandInput.Options.FindByOptionSchema(optionSchema);

                //If no option input is available fall back to environment variable values
                if (optionInput == null)
                {
                    var environmentValues = _environmentVariablesProvider.GetValues(optionSchema.EnvironmentVariableName);

                    //If the environment variable values are also missing skip this option
                    if (environmentValues == null)
                        continue;

                    //Make a new CommandOptionInput using environment variables
                    optionInput = new CommandOptionInput(optionSchema.EnvironmentVariableName, environmentValues);
                }

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