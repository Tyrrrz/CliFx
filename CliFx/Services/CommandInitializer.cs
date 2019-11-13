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
        private readonly IEnvironmentVariablesParser _environmentVariablesParser;

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer(ICommandOptionInputConverter commandOptionInputConverter, IEnvironmentVariablesParser environmentVariablesParser)
        {
            _commandOptionInputConverter = commandOptionInputConverter;
            _environmentVariablesParser = environmentVariablesParser;
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer(IEnvironmentVariablesParser environmentVariablesParser)
            : this(new CommandOptionInputConverter(), environmentVariablesParser)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer()
            : this(new CommandOptionInputConverter(), new EnvironmentVariablesParser())
        {
        }

        /// <inheritdoc />
        public void InitializeCommand(ICommand command, CommandSchema commandSchema, CommandInput commandInput)
        {
            // Keep track of unset required options to report an error at a later stage
            var unsetRequiredOptions = commandSchema.Options.Where(o => o.IsRequired).ToList();

            //Set command options
            foreach (var optionSchema in commandSchema.Options)
            {
                // Ignore special options that are not backed by a property
                if (optionSchema.Property == null)
                    continue;

                //Find matching option input
                var optionInput = commandInput.Options.FindByOptionSchema(optionSchema);

                //If no option input is available fall back to environment variable values
                if (optionInput == null && !string.IsNullOrWhiteSpace(optionSchema.EnvironmentVariableName))
                {
                    var fallbackEnvironmentVariableExists = commandInput.EnvironmentVariables.ContainsKey(optionSchema.EnvironmentVariableName!);

                    //If no environment variable is found or there is no valid value for this option skip it
                    if (!fallbackEnvironmentVariableExists || string.IsNullOrWhiteSpace(commandInput.EnvironmentVariables[optionSchema.EnvironmentVariableName!]))
                        continue;

                    optionInput = _environmentVariablesParser.GetCommandOptionInputFromEnvironmentVariable(commandInput.EnvironmentVariables[optionSchema.EnvironmentVariableName!], optionSchema);
                }

                //No fallback available and no option input was specified, skip option
                if (optionInput == null)
                    continue;

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