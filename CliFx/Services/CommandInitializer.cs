using System;
using System.Collections.Generic;
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
        private readonly ICommandInputConverter _commandInputConverter;
        private readonly IEnvironmentVariablesParser _environmentVariablesParser;

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer(ICommandInputConverter commandInputConverter, IEnvironmentVariablesParser environmentVariablesParser)
        {
            _commandInputConverter = commandInputConverter;
            _environmentVariablesParser = environmentVariablesParser;
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer(IEnvironmentVariablesParser environmentVariablesParser)
            : this(new CommandInputConverter(), environmentVariablesParser)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInitializer"/>.
        /// </summary>
        public CommandInitializer()
            : this(new CommandInputConverter(), new EnvironmentVariablesParser())
        {
        }

        private void InitializeCommandOptions(ICommand command, TargetCommandSchema targetCommandSchema)
        {
            if (targetCommandSchema.Schema is null)
            {
                throw new ArgumentException("Cannot initialize command without a schema.");
            }

            // Keep track of unset required options to report an error at a later stage
            var unsetRequiredOptions = targetCommandSchema.Schema.Options.Where(o => o.IsRequired).ToList();

            //Set command options
            foreach (var optionSchema in targetCommandSchema.Schema.Options)
            {
                // Ignore special options that are not backed by a property
                if (optionSchema.Property == null)
                    continue;

                //Find matching option input
                var optionInput = targetCommandSchema.CommandInput.Options.FindByOptionSchema(optionSchema);

                //If no option input is available fall back to environment variable values
                if (optionInput == null && !string.IsNullOrWhiteSpace(optionSchema.EnvironmentVariableName))
                {
                    var fallbackEnvironmentVariableExists = targetCommandSchema.CommandInput.EnvironmentVariables.ContainsKey(optionSchema.EnvironmentVariableName!);

                    //If no environment variable is found or there is no valid value for this option skip it
                    if (!fallbackEnvironmentVariableExists || string.IsNullOrWhiteSpace(targetCommandSchema.CommandInput.EnvironmentVariables[optionSchema.EnvironmentVariableName!]))
                        continue;

                    optionInput = _environmentVariablesParser.GetCommandOptionInputFromEnvironmentVariable(targetCommandSchema.CommandInput.EnvironmentVariables[optionSchema.EnvironmentVariableName!], optionSchema);
                }

                //No fallback available and no option input was specified, skip option
                if (optionInput == null)
                    continue;

                var convertedValue = _commandInputConverter.ConvertOptionInput(optionInput, optionSchema.Property.PropertyType);

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

        private void InitializeCommandArguments(ICommand command, TargetCommandSchema targetCommandSchema)
        {
            if (targetCommandSchema.Schema is null)
            {
                throw new ArgumentException("Cannot initialize command without a schema.");
            }

            // Keep track of unset required options to report an error at a later stage
            var unsetRequiredArguments = targetCommandSchema.Schema.Arguments
                .Where(o => o.IsRequired)
                .ToList();
            var orderedArgumentSchemas = targetCommandSchema.Schema.Arguments.Ordered();

            using var positionalArgumentEnumerator = targetCommandSchema.PositionalArgumentsInput.GetEnumerator();

            foreach (var argumentSchema in orderedArgumentSchemas)
            {
                if (!positionalArgumentEnumerator.MoveNext())
                {
                    // No more positional arguments left - remaining argument properties stay unset
                    break;
                }

                var convertedValue = _commandInputConverter.ConvertArgumentInput(positionalArgumentEnumerator, argumentSchema.Property.PropertyType);

                // Set value of underlying property
                argumentSchema.Property.SetValue(command, convertedValue);

                // Mark this required argument as set
                if (argumentSchema.IsRequired)
                    unsetRequiredArguments.Remove(argumentSchema);
            }

            // Throw if any of the required options were not set
            if (unsetRequiredArguments.Any())
            {
                throw new CliFxException($"One or more required options were not set: {unsetRequiredArguments.JoinToString(", ")}.");
            }
        }

        /// <inheritdoc />
        public void InitializeCommand(ICommand command, TargetCommandSchema targetCommandSchema)
        {
            InitializeCommandOptions(command, targetCommandSchema);
            InitializeCommandArguments(command, targetCommandSchema);
        }
    }
}